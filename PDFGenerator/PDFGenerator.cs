using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PDFGenerator
{
    public static class PDFGenerator
    {


        [FunctionName("PDFGenerator")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            ILogger log, ExecutionContext executionContext)
        {
            log.LogInformation("Processing PDF Request");

            string client = req.Query["client"];
            string templateType = req.Query["type"];

            string templateTypeFile = String.Concat(templateType, ".html");
            var templatePath = System.IO.Path.Combine(executionContext.FunctionDirectory, "../../../../", "clientTemplates", client, templateTypeFile);
            var templateHtml = System.IO.File.ReadAllText(templatePath);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Dictionary<string, string> receivedjson = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody);

            int i = 0;
            foreach (KeyValuePair<string, string> kvp in receivedjson)
            {
                i++;
                string target = String.Concat("{{", kvp.Key, "}}");
                Console.WriteLine($"Target {target}. Replacement value is : {kvp.Value}");

               templateHtml = templateHtml.Replace(target, kvp.Value);
            }

            var Renderer = new IronPdf.HtmlToPdf();



            byte[] data = Renderer.RenderHtmlAsPdf(templateHtml).BinaryData;
            log.LogInformation($"PDF Generated. Length={data.Length}");

            var res = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(data)
            };

            res.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            res.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline");

            return res;
        }



    }
}
