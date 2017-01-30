#Continuum


>plural continuaplay \-yü-ə\ also continuums
>
>1: a coherent whole characterized as a collection, sequence, or progression of values or elements varying by minute degrees “good” and “bad” … stand at opposite ends of a continuum instead of describing the two halves of a line — Wayne Shumaker
>
>2: the set of real numbers including both the rationals and the irrationals; broadly : a compact set which cannot be separated into two sets neither of which contains a limit point of the other


In modern SOA or micro services architectures we spread processing and handling of user request across multiple machines and instances making debugging and troubleshooting harder. This tool allows all instances to stream log
entries to a centralized service. The developers can on their side connect to the Continuum UI and watch all entries flow through the system, pause if anything interesting happens to investigate further.

##Service setup:

* Download the web deployment package here: [Web Deploy Package](https://github.com/JonasSyrstad/Stardust.Interstellar.Rest/tree/master/ContinuumInstall)

* Follow the instructions: [How to: Install a Deployment Package](https://msdn.microsoft.com/en-us/library/ff356104(v=vs.110).aspx)

* Add your projects and environments in the web.config file. 


##Usage:

Direct
```CS
LogStreamConfig.ApiKey = apiKeyFromConfig;//only once per app instance
await ProxyFactory.CreateInstance<ILogStream>(serviceUrl).AddAddStream(projectIdentifier, environmentName,new StreamItem{});
```

Wrapper client:

Initialization
```CS
//Initialization, can also be added in config AppSettings section (see trailing comment);

ContinuumClient.BaseUrl=serviceUrl; // <add key="continuum:apiUrl" value="https://http://continuumdemo.azurewebsites.net/"/>
ContinuumClient.Project=projectIdentifier; // <add key="continuum:project"  value="test"/>
ContinuumClient.Environment=environmentName; // <add key="continuum:environment" value="test"/>
ContinuumClient.ApiKey=apiKeyFromConfig; // <add key="continuum:apiKey" value="test123"/>
```
Add to stream
```CS

//Write to log stream. This call does not block the execution of the main process.
ContinuumClient.AddStream(new StreamItem{ Message="this is a new log entry. added as a sample", LogLevel=LogLevels.Debug, UserName=User.Name, CorrelationToken=ActivityId});

```

