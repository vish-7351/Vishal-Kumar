using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

class SimpleWebServer
{
    static void Main()
    {
        int port = 8080;
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(string.Format("http://localhost:{0}/", port));
        listener.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", port));
        try {
            listener.Start();
            Console.WriteLine(string.Format("Server started! Please open your browser and go to: http://localhost:{0}/", port));

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                Task.Run(() => ProcessRequest(context));
            }
        } catch (Exception ex) {
            Console.WriteLine(string.Format("Error starting server: {0}", ex.Message));
            Console.WriteLine("Try changing the port or running as administrator.");
        }
    }

    static void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        try
        {
            string localPath = request.Url.LocalPath.TrimStart('/');
            if (string.IsNullOrEmpty(localPath))
            {
                localPath = "index.html";
            }

            localPath = localPath.Replace("..", "");
            string filePath = Path.Combine(Environment.CurrentDirectory, localPath);

            if (File.Exists(filePath))
            {
                string ext = Path.GetExtension(filePath).ToLower();
                string contentType = "application/octet-stream";
                switch (ext)
                {
                    case ".html": contentType = "text/html"; break;
                    case ".css": contentType = "text/css"; break;
                    case ".js": contentType = "application/javascript"; break;
                    case ".jpg":
                    case ".jpeg": contentType = "image/jpeg"; break;
                    case ".png": contentType = "image/png"; break;
                }
                response.ContentType = contentType;

                using (FileStream fs = File.OpenRead(filePath))
                {
                    response.ContentLength64 = fs.Length;
                    fs.CopyTo(response.OutputStream);
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
        catch (Exception)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            response.OutputStream.Close();
        }
    }
}
