using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MOZ_UPGRADE.Interfaces
{
    public sealed class PortalDocumentSoapService : IDisposable
    {
        private readonly HttpClient _http;
        private readonly string _endpoint;

        private const string SoapNs = "http://schemas.xmlsoap.org/soap/envelope/";
        private const string Tns = "urn:microsoft-dynamics-schemas/codeunit/PortalIntegration";

        public PortalDocumentSoapService(string endpointUrl, NetworkCredential credential, TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(endpointUrl))
                throw new ArgumentNullException(nameof(endpointUrl));

            _endpoint = endpointUrl.TrimEnd('/');

            var handler = new HttpClientHandler
            {
                UseDefaultCredentials = false,
                PreAuthenticate = true,
                Credentials = credential
            };

            _http = new HttpClient(handler)
            {
                Timeout = timeout ?? TimeSpan.FromMinutes(2)
            };
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
        }

        /// <summary>
        /// Attach a document to a PEP record in ERP
        /// </summary>
        public async Task<string> AttachPepDocumentAsync(
            string pepNo,
            int pepRecordNo,
            byte[] fileBytes,
            string fileName,
            string documentCategory,
            string documentType,
            DateTime expiryDate,
            string requestType)
        {
            var base64 = Convert.ToBase64String(fileBytes ?? Array.Empty<byte>());

            var body = new XElement(XName.Get("AttachPepDocument", Tns),
                new XElement(XName.Get("pepNo", Tns), pepNo ?? string.Empty),
                new XElement(XName.Get("pepRecordNo", Tns), pepRecordNo),
                new XElement(XName.Get("base64Content", Tns), base64),
                new XElement(XName.Get("fileName", Tns), string.IsNullOrWhiteSpace(fileName) ? "file" : fileName),
                new XElement(XName.Get("documentCategory", Tns), string.IsNullOrWhiteSpace(documentCategory) ? "General" : documentCategory),
                new XElement(XName.Get("documentCode", Tns), string.IsNullOrWhiteSpace(documentType) ? "Attachment" : documentType),
                new XElement(XName.Get("expiryDate", Tns), expiryDate.ToString("yyyy-MM-dd")),
                new XElement(XName.Get("requestType", Tns), string.IsNullOrWhiteSpace(requestType) ? "KYC" : requestType)
            );

            return await SendAsync($"{Tns}:AttachPepDocument", body, "AttachPepDocument_Result");
        }

        /// <summary>
        /// Generic SOAP envelope sender
        /// </summary>
        private async Task<string> SendAsync(string soapAction, XElement bodyElement, string resultElementLocalName)
        {
            var envelope = new XDocument(
                new XElement(XName.Get("Envelope", SoapNs),
                    new XAttribute(XNamespace.Xmlns + "soap", SoapNs),
                    new XElement(XName.Get("Body", SoapNs), bodyElement)
                )
            );

            var content = new StringContent(envelope.ToString(SaveOptions.DisableFormatting));
            content.Headers.ContentType = new MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" };
            content.Headers.Add("SOAPAction", $"\"{soapAction}\"");

            using var resp = await _http.PostAsync(_endpoint, content);
            var raw = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"SOAP call failed ({resp.StatusCode}). Body: {raw}");

            var doc = XDocument.Parse(raw);
            var soap = (XNamespace)SoapNs;
            var tns = (XNamespace)Tns;

            var result = doc.Root?
                .Element(soap + "Body")?
                .Element(tns + resultElementLocalName)?
                .Element(tns + "return_value")?.Value;

            if (string.IsNullOrWhiteSpace(result))
                throw new InvalidOperationException("SOAP response missing return_value.");

            return result!;
        }

        public void Dispose() => _http?.Dispose();
    }
}
