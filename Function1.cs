
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
using Newtonsoft.Json.Linq;
using System;

namespace pdfGenerator
{
    public static class Function1
    {
        [FunctionName("pdfGenerator")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, TraceWriter log, ExecutionContext executionContext)
        {
            string name = req.Query["url"];
            log.Error(name);
            string[] splitUrl = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

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
            var templatePath = System.IO.Path.Combine(executionContext.FunctionDirectory, splitUrl[0], splitUrl[1]);

            var templateHtml = System.IO.File.ReadAllText(templatePath);
            
            log.Error(templateHtml);

            JObject myJObject;

            //Grab POST body

            using (StreamReader stream = new StreamReader(req.Body))
            {
                string requestBody = stream.ReadToEnd();
                myJObject = JObject.Parse(requestBody);
            }

            //Replace HTML values based on incoming JSON
            templateHtml.Replace("{{letterFolio}}", myJObject.SelectToken("$.responseLetter.letterFolio").Value<string>());
            //Convert value replaced HTML to PDF

            HttpResponseMessage response;

            //Convert URL to PDF 
            PdfDocument document = htmlConverter.Convert(templateHtml);

            using (MemoryStream ms = new MemoryStream())
            {
                //Save the PDF document  
                document.Save(ms);
                ms.Position = 0;

                response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(ms.ToArray())
                };
            }
                
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "TBD.pdf"
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            
            return response;
        }
    }
}
