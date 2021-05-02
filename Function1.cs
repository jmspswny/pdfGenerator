
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Syncfusion.HtmlConverter;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using Syncfusion.Pdf;
using System.Collections.Specialized;

namespace pdfGenerator
{
    public static class Function1
    {
        [FunctionName("pdfGenerator")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, TraceWriter log, ExecutionContext executionContext)
        {

            string name = req.Query["url"];
            log.Error(name);
            string[] splitUrl = name.Split('/');

            //Initialize HTML to PDF converter  
            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
            WebKitConverterSettings settings = new WebKitConverterSettings();

            //Set WebKit path
            settings.WebKitPath = Path.Combine(executionContext.FunctionAppDirectory, "QtBinariesWindows");
            //Assign WebKit settings to HTML converter 
            htmlConverter.ConverterSettings = settings;

            //Load template based on requested client and requested template
            //var binDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //var templatePath = System.IO.Path.Combine(binDirectory, "../", splitUrl[0], "/", splitUrl[1],".html");
            var templatePath = System.IO.Path.Combine(executionContext.FunctionDirectory, "/", splitUrl[0], "/", splitUrl[1], ".html");

            var templateHtml = System.IO.File.ReadAllText(templatePath);
            
            log.Error(templateHtml);

            //Grab POST body

            using (StreamReader stream = new StreamReader(HttpContext.Request.Body))
            {
                string requestBody = stream.ReadToEnd();
            }

            //Replace HTML values based on incoming JSON
            templateHtml.Replace("{{letterFolio}}", myJObject.SelectToken("$.responseLetter.letterFolio").Value<string>()) 
            //Convert value replaced HTML to PDF

            //Convert URL to PDF 
            PdfDocument document = htmlConverter.Convert(templateHtml);
            MemoryStream ms = new MemoryStream();
            //Save the PDF document  
            document.Save(ms);
            ms.Position = 0;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(ms.ToArray());
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "TBD.pdf"
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

            return response;
        }

    }
}
