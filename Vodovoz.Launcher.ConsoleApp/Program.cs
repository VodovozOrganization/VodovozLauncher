using System.Diagnostics;
using System.Reflection;
using System.Web;

const string _protocolPrefix = "vodovoz://";
const string _waterDeliveryDirectoryName = "WaterDelivery";
const string _waterDeliveryExecutableName = "Vodovoz.exe";
const string _currentReleaseFileName = "current.lock";
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

var lockFilePath = $"{directoryBasePath}\\{_currentReleaseFileName}";

var applicationPath = string.Empty;

if(File.Exists(lockFilePath))
{
    var streamReader = new StreamReader(lockFilePath);

    var currentApplicationDirectory = streamReader.ReadToEnd();

    streamReader.Close();

    if(!string.IsNullOrWhiteSpace(currentApplicationDirectory))
    {
        applicationPath = $"{directoryBasePath}\\{currentApplicationDirectory}\\{_waterDeliveryExecutableName}";
    }
}

if(string.IsNullOrWhiteSpace(applicationPath) || !File.Exists(applicationPath))
{
    Console.WriteLine("Программа обновляется, повторите позднее");
    Console.ReadKey();
    return;
}

var arguments = $"-d \"{databaseConnectionName}\"";
Process.Start(applicationPath, arguments);
