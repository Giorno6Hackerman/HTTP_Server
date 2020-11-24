using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;


namespace HTTPServer
{
    class Client
    {
        // Отправка страницы с ошибкой
        private void SendError(TcpClient Client, int Code)
        {
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
            Console.WriteLine(Str);
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            Client.Close();
        }
    

        public Client(TcpClient Client)
        {
            // Запрос клиента
            string Request = "";
            // Буфер для хранения принятых от клиента данных
            byte[] Buffer = new byte[1024];
            // Переменная для хранения количества байт, принятых от клиента
            int Count;

            while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
            {
                Request += Encoding.ASCII.GetString(Buffer, 0, Count);
                if (Request.IndexOf("\r\n\r\n") >= 0)
                {
                    break;
                }
            }
            Console.WriteLine(Request);
   
            // Парсим строку запроса с использованием регулярных выражений
            // При этом отсекаем все переменные GET-запроса
            Match ReqMatch = Regex.Match(Request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");
   
            if (ReqMatch == Match.Empty)
            {
                // Ошибка 400 - неверный запрос
                SendError(Client, 400);
                return;
            }
   
            // Строка запроса
            string RequestUri = ReqMatch.Groups[1].Value;
            // Преобразование экранированных символов
            RequestUri = Uri.UnescapeDataString(RequestUri);
   
            if (RequestUri.IndexOf("..") >= 0)
            {
                SendError(Client, 400);
                return;
            }
   
            if (RequestUri.EndsWith("/"))
            {
                RequestUri += "LabMain.html";
            }
   
            string FilePath = "D:/prog/Content/" + RequestUri;
   
            // Если в папке не существует данного файла, ошибка 404
            if (!File.Exists(FilePath))
            {
                SendError(Client, 404);
                return;
            }
   
            // Расширение файла
            string Extension = RequestUri.Substring(RequestUri.LastIndexOf('.'));
   
            // Тип содержимого
            string ContentType = "";
   
            // Определение типа содержимого по расширению файла
            switch (Extension)
            {
                case ".htm":
                case ".html":
                    ContentType = "text/html";
                    break;
                case ".css":
                    ContentType = "text/stylesheet";
                    break;
                case ".js":
                    ContentType = "text/javascript";
                    break;
                case ".jpg":
                    ContentType = "image/jpeg";
                    break;
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".bmp":
                    ContentType = "image/" + Extension.Substring(1);
                    break;
                default:
                    if (Extension.Length > 1)
                    {
                        ContentType = "application/" + Extension.Substring(1);
                    }
                    else
                    {
                        ContentType = "application/unknown";
                    }
                    break;
            }
   
            FileStream FS;
            try
            {
                FS = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {
                SendError(Client, 500);
                return;
            }

            DateTime now = DateTime.Now;
            string date = String.Format("Date: {0:R}\n", now);
            // Посылаем заголовки
            string Headers = "HTTP/1.1 200 OK\n" + date + "Content-Type: " + ContentType + "\nContent-Length: " + FS.Length + "\n\n";
            Console.WriteLine(Headers);
            byte[] HeadersBuffer = Encoding.ASCII.GetBytes(Headers);
            Client.GetStream().Write(HeadersBuffer, 0, HeadersBuffer.Length);

            while (FS.Position<FS.Length)
            {
                Count = FS.Read(Buffer, 0, Buffer.Length);
                Client.GetStream().Write(Buffer, 0, Count);
            }

            FS.Close();
            Client.Close();
        }
    }
}
