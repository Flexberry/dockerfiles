# Description

This image supports the functionality [apache-mono (version 4.6.2.7)](https://github.com/Flexberry/dockerfiles/blob/master/alt.p8-apache2/README.md) of the server and is the base image for creating specialized images of launching `apache-mono` applications.

## Environment Variables

- `XMLTEMPLATES` - list of corrected XML files.
- Variables listed in corrected XML files.

## Functional

Starting from version `4.6.2.7-1.3.0`, the ability to customize the arguments of tags of XML files is supported.
If the argument tag of the XML file contains a pattern like `%%% VARIABLE %%`,
then the value of this argument is replaced with the value of the specified variable.

The list of corrected files is specified in the environment variable `XMLTEMPLATES`.
File names in the list are separated by spaces.
If the variable `XMLTEMPLATES` is billed or not initialized, no file adjustments are made

When launching the image container, all variables specified in the templates should be defined.
If at least one variable is not defined or has an empty value, the container terminates its work.
Environment variables can be set in the following ways:
- initialization in the `ENV` statement of the` Dockerfile` file when creating an image. For example:
  ```
  FROM flexberry / alt.p8-apache2-mono: 4.6.2.7-1.3
  ...
  ENV XMLTEMPLATES "/var/www/web-api/app/Web.config"
  ...
  ```
- initialization when the container is started via the `-e` flag. For example:
  ```
  $ docker run -e "XMLTEMPLATES = / var / www / web-api / app / Web.config" ...
  ```
  
- initialization to a YML file.
For example:
  ```
  services:
  monoservice:
    image: ...
      environment:
        - XMLTEMPLATES = / var / www / web-api / app / Web.config
  ```

>RECOMMENDED A VARIABLE `XMLTEMPLATES` INITIALIZING in a SUBSCRIPTION Dockerfile. VARIABLES USED FOR CORRECTION CERTAINLY SHOULD BE SPECIFIED WHEN STARTING A CONTAINER / SERVICE IN PEMETS OR YML FILE.

When the container / service is started, the following commands of the `CMD` operator are executed:
```
Cmd /bin/change_XMLconfig_from_env.sh && \
    / usr / sbin / httpd2 -D NO_DETACH -k start
```

The `change_XMLconfig_from_env.sh` script in the files listed in the` XMLTEMPLATES 'variable makes an adjustment to the arguments.
Upon successful completion, launches the WEB service that initiates the launch of the `mono` service.

If you need to start additional services in child images, you must override the `CMD` operator in` Dockerfile`.

## Example

Consider adjusting the XML configuration file `/var/www/web-api/app/Web.config`:
```
<? xml version = "1.0" encoding = "utf-8"?>
<configuration>
  <appSettings>
    <add key = "DefaultConnectionStringName" value = "DefConnStr" />
    <add key = "ActivityServicesApiUrl" value = "%% ACTIVITY_SERVICES_API_URL %%" />
  </ appSettings>
  <connectionStrings>
    <add name = "DefConnStr" connectionString = "%% BPM_CONNECTION_STRING %%" />
    <add name = "AgentSyncConnStr" connectionString = "%% DMS_CONNECTION_STRING %%" />
  </ connectionStrings>
  <quartz>
    <add key = "quartz.scheduler.instanceName" value = "FlowpointFlexberryTimerClient" />
    <add key = "quartz.scheduler.instanceId" value = "AUTO" />
    <add key = "quartz.scheduler.proxy" value = "true" />
    <add key = "quartz.scheduler.proxy.address" value = "%% BPM_TIMER_URL %%" />
    <add key = "quartz.threadPool.type" value = "Quartz.Simpl.SimpleThreadPool, Quartz" />
    <add key = "quartz.threadPool.threadCount" value = "0" />
  </ quartz>
</ configuration>
```

The name of the configuration file to be adjusted is specified in the `XMLTEMPLATES` variable in the` Dockerfile` file when creating the image:
  ```
  FROM flexberry / alt.p8-apache2-mono: 4.6.2.7-1.3
  ...
  ENV XMLTEMPLATES "/var/www/web-api/app/Web.config"
  ...
  ```

Variable values
`ACTIVITY_SERVICES_API_URL`,` JBPM_API_URL`, `BPM_CONNECTION_STRING`,` DMS_CONNECTION_STRING`, `BPM_TIMER_URL` are specified in the YML service description file:
```
services:
monoservice:
  image: ...
    environment:
      - ACTIVITY_SERVICES_API_URL = http: // ...
      - JBPM_API_URL = http: // ...
      - BPM_CONNECTION_STRING = Server = SrvBPM; Port = 5432; ...
      - DMS_CONNECTION_STRING = Server = SrvDMS; Port = 5432; ...
      - BPM_TIMER_URL = http: // ...
```
