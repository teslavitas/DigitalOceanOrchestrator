using DigitalOcean.API;

namespace DigitalOceanOrchestrator;

internal class DropletService
{
    const string TagActivated = "Activated";
    private readonly int _totalDroplets;
    private readonly DigitalOceanClient _client;
    private readonly string _sshFingerprint;
    private readonly Random _random;
    private readonly SshService _sshService;
    private readonly int _deleteDropletsAfterMinutes;
    private readonly string _tag;
    public DropletService(Settings settings, SshService sshService)
    {
        _client = new DigitalOceanClient(settings.DigitalOceanToken);
        _totalDroplets = settings.TotalDroplets;
        _sshFingerprint = settings.SshFingerprint;
        _deleteDropletsAfterMinutes = settings.DeleteDropletsAfterMinutes;
        _sshService = sshService;
        _tag = settings.Tag;
        _random = new Random();
    }
    public async Task Run()
    {
        var droplets = await _client.Droplets.GetAllByTag(_tag);

        var dropletsToDelete = droplets.Where(d => ShouldDeleteDroplet(d)).ToList();
        var dropletsToActivate = droplets.Where(d => ShouldActivateDroplet(d)).ToList();

        int dropletsCountToCreate = _totalDroplets - droplets.Count;

        // delete dropletes
        Task.WaitAll(dropletsToDelete.Select(droplet => DeleteDroplet(droplet.Id)).ToArray());

        // create droplets
        if (dropletsCountToCreate > 0)
        {
            Task.WaitAll(new int[dropletsCountToCreate].Select(_ => CreateDroplet()).ToArray());
        }

        //activate droplets 
        Task.WaitAll(dropletsToActivate.Select(droplet => ActivateDroplet(droplet)).ToArray());
    }

    public async Task PrepareTags()
    {
        var tags = await _client.Tags.GetAll();
        var resistanceTag = tags.FirstOrDefault(t => t.Name == _tag);
        if (resistanceTag == null)
        {
            resistanceTag = await _client.Tags.Create(_tag);
        }

        var activatedTag = tags.FirstOrDefault(t => t.Name == TagActivated);
        if (activatedTag == null)
        {
            activatedTag = await _client.Tags.Create(TagActivated);
        }
    }


    private bool ShouldDeleteDroplet(DigitalOcean.API.Models.Responses.Droplet droplet)
    {
        return droplet.CreatedAt < DateTime.UtcNow.AddMinutes(-_deleteDropletsAfterMinutes);
    }

    private bool ShouldActivateDroplet(DigitalOcean.API.Models.Responses.Droplet droplet)
    {
        return droplet.Status == "active" && !droplet.Tags.Contains(TagActivated);
    }

    private async Task CreateDroplet()
    {
        var request = new DigitalOcean.API.Models.Requests.Droplet
        {
            Name = "resistance",
            Region = GetRandomRegion(),
            Size = "s-1vcpu-1gb",
            Image = "docker-20-04",
            SshKeys = new List<object> { _sshFingerprint },
            Backups = false,
            Ipv6 = false,
            Tags = new List<string> { _tag }
        };
        var droplet = await _client.Droplets.Create(request);
        LogHelper.Log($"created droplet {droplet.Id}");
    }

    private async Task ActivateDroplet(DigitalOcean.API.Models.Responses.Droplet droplet)
    {
        // connect by SSH and run commands
        await _sshService.RunCommands(droplet.Networks.V4.First(n => n.Type == "public").IpAddress);

        //Assign a tag
        var request = new DigitalOcean.API.Models.Requests.TagResources()
        {
            Resources = new List<DigitalOcean.API.Models.Requests.TagResource>()
        {
            new DigitalOcean.API.Models.Requests.TagResource() { Id = droplet.Id.ToString(), Type = "droplet" }
        }
        };

        await _client.Tags.Tag(TagActivated, request);
        LogHelper.Log($"activated droplet {droplet.Id}");
    }

    private async Task DeleteDroplet(long dropletId)
    {
        await _client.Droplets.Delete(dropletId);
        LogHelper.Log($"deleted droplet {dropletId}");
    }

    private string GetRandomRegion()
    {
        var number = _random.Next(5);
        return number switch
        {
            0 => "ams3",
            1 => "lon1",
            2 => "fra1",
            3 => "sgp1",
            _ => "blr1",
        };
    }
}

