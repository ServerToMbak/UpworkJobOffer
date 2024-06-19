using System.Net;

namespace UpworkJob
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string _ftpServer = "ftp://example.com";
        private readonly string _ftpUsername = "username";
        private readonly string _ftpPassword = "password";
        private readonly string _remoteDirectory = "/path/to/css/files";
        private readonly string _localDirectory = @"C:\Local\Path\To\Save\Files";
        private readonly TimeSpan _interval = TimeSpan.FromHours(4);    

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    DownloadCssFiles(_ftpServer, _ftpUsername, _ftpPassword, _remoteDirectory, _localDirectory);
                    _logger.LogInformation("CSV files downloaded successfully at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while downloading CSS files.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private void DownloadCssFiles(string ftpServer, string ftpUsername, string ftpPassword, string remoteDirectory, string localDirectory)
        {
            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(ftpServer + remoteDirectory);
            listRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            listRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            using (FtpWebResponse listResponse = (FtpWebResponse)listRequest.GetResponse())
            using (StreamReader listReader = new StreamReader(listResponse.GetResponseStream()))
            {
                while (!listReader.EndOfStream)
                {
                    string fileName = listReader.ReadLine();
                    if (fileName.EndsWith(".csv"))
                    {
                        string remoteFilePath = ftpServer + remoteDirectory + "/" + fileName;
                        string localFilePath = Path.Combine(localDirectory, fileName);

                        DownloadFile(remoteFilePath, localFilePath, ftpUsername, ftpPassword);
                    }
                }
            }
        }
        private void DownloadFile(string remoteFilePath, string localFilePath, string ftpUsername, string ftpPassword)
        {
            FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(remoteFilePath);
            downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            downloadRequest.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

            using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
            using (Stream downloadStream = downloadResponse.GetResponseStream())
            using (FileStream localFileStream = new FileStream(localFilePath, FileMode.Create))
            {
                downloadStream.CopyTo(localFileStream);
            }
        }
    }
}
