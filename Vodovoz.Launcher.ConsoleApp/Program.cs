using System.Diagnostics;
using System.Reflection;
using System.Web;

const string _protocolPrefix = "vodovoz://";
const string _waterDeliveryDirectoryName = "WaterDelivery";
const string _waterDeliveryExecutableName = "Vodovoz.exe";
const string _lockFileName = "update.lock";
const string _defaultDatabaseConnectionName = "Рабочая";

#region Override working directory

var strExeFilePath = Assembly.GetExecutingAssembly().Location;

var executableDirectory = Path.GetDirectoryName(strExeFilePath);

if(executableDirectory is null)
{
    throw new InvalidOperationException("Executable dirtectory can't be null");
}

Directory.SetCurrentDirectory(executableDirectory);

#endregion Override working directory

var uriString = args.First();

// URL scheema: vodovoz://{applicationType}/{databaseConnectionName}
// applicationType: Test, Work

var query = HttpUtility.UrlDecode(uriString)
    .Replace(_protocolPrefix, string.Empty)
    .Split("/");

var applicationType = query[0];
var databaseConnectionName = query.Length == 1 ? _defaultDatabaseConnectionName : query[1];

var directoryBasePath = Path.GetFullPath(
    $"{Directory.GetCurrentDirectory()}\\..\\{_waterDeliveryDirectoryName}\\{applicationType}");

var programDirectories = Directory.GetDirectories(directoryBasePath);

var programs = new Dictionary<string, DateTime>();

foreach(var programDirectory in programDirectories)
{
    programs.Add($"{programDirectory}\\{_waterDeliveryExecutableName}",
        File.GetCreationTime($"{programDirectory}\\{_waterDeliveryExecutableName}"));
}

var lockFilePath = $"{directoryBasePath}\\{_lockFileName}";

if(File.Exists(lockFilePath))
{
    string applicationLock = string.Empty;

    StreamReader streamReader = new StreamReader(lockFilePath);

    applicationLock = streamReader.ReadToEnd();

    streamReader.Close();

    if (!string.IsNullOrWhiteSpace(applicationLock))
    {
        programs.Remove(applicationLock);
    }
}

var path = programs.OrderByDescending(x => x.Value).First().Key;

var arguments = $"-d \"{databaseConnectionName}\"";

Process.Start(path, arguments);
