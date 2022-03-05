Program automates creation and removal of Digital Ocen (DO) droplets with a given interval.

Before using the tool try to create droplets manually as described here: https://dou.ua/forums/topic/36874/.

To start using the program do following: 
1. Download the program or build it from sources.
2. Create an Access Token in DO by going to API - Tokens/Keys - Generate New Token
3. Create an SSH key on your computer by running this in terminal: ssh-keygen ssh-keygen -m PEM
   It will create 2 files with private and public keys, use public key in step 4
4. Register SSH key in DO by going to Settings - Security - Add SSH key
5. Change settings in the appsettings.json file:
    DigitalOceanToken - token from step 2
    TotalDroplets - number of droplets you want to maintain
    DeleteDropletsAfterMinutes - maximum age of a droplet before it is deleted
    Tag - the program will only look for droplets with this tag, it will ignore other droplets
    SshFingerprint - SSH key fingerprint from DO, created in step 4
    SshPrivateKeyFilePath - path to a file with your private SSH key generated in step 3
    SshPrivateKeyPassPhrase - optional, if you used a pass phrase in step 3, put it here
    SshCommands - startup commands that will be executed in the droplet terminal after droplet is created. Program will wait until one command is finished before sending the next command except for the last command - it is not awaited.
6. Run the program.

Dropletes will be periodically recreated while program is running. It will log activities to console.

The program uses the smallest available droplet size s-1vcpu-1gb and a docker image: https://marketplace.digitalocean.com/apps/docker

