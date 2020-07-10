using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.BarCode;
using Aspose.BarCode.Generation;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using PDF_Creator.Models;

namespace PDF_Creator.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public FileResult Index(string editor1, string codeText, string barcodeType)
        {
            // generate a barcode
            string barcodeImagePath = Path.Combine("wwwroot/barcodes/", Guid.NewGuid() + ".png");
            SymbologyEncodeType type = GetBarcodeSymbology(barcodeType);
            BarcodeGenerator generator = new BarcodeGenerator(type, codeText);
            generator.Parameters.BackColor = System.Drawing.Color.Transparent;
            // set resolution of the barcode image
            generator.Parameters.Resolution = 200;
            // generate barcode
            generator.Save(barcodeImagePath, BarCodeImageFormat.Png);

            // create a unique file name for PDF
            string fileName = Guid.NewGuid() + ".pdf";
            // convert HTML text to stream
            byte[] byteArray = Encoding.UTF8.GetBytes(editor1);
            // generate PDF from the HTML
            MemoryStream stream = new MemoryStream(byteArray);
            HtmlLoadOptions options = new HtmlLoadOptions();
            Document pdfDocument = new Document(stream, options);

            // add barcode image to the generated PDF 
            pdfDocument = InsertImage(pdfDocument, barcodeImagePath);

            // create memory stream for the PDF file
            Stream outputStream = new MemoryStream();
            // save PDF to output stream
            pdfDocument.Save(outputStream);

            // return generated PDF file
            return File(outputStream, System.Net.Mime.MediaTypeNames.Application.Pdf, fileName);
        }
        private SymbologyEncodeType GetBarcodeSymbology(string symbology)
        {
            if (symbology.ToLower() == "qr")
                return EncodeTypes.QR;
            else if (symbology.ToLower() == "code128")
                return EncodeTypes.Code128;
            else if (symbology.ToLower() == "code11")
                return EncodeTypes.Code11;
            else if (symbology.ToLower() == "pdf417")
                return EncodeTypes.Pdf417;
            else if (symbology.ToLower() == "datamatrix")
                return EncodeTypes.DataMatrix;
            else
                return EncodeTypes.Code128; // default barcode type
        }
        private Document InsertImage(Document document, string barcodeImagePath)
        {
            // get page from Pages collection of PDF file
            Aspose.Pdf.Page page = document.Pages[1];
            // create an image instance
            Aspose.Pdf.Image img = new Aspose.Pdf.Image();
            img.IsInLineParagraph = true;
            // set Image Width and Height in Points
            img.FixWidth = 100;
            img.FixHeight = 100;
            img.HorizontalAlignment = HorizontalAlignment.Right;
            img.VerticalAlignment = VerticalAlignment.Top;
            // set image type as SVG
            img.FileType = Aspose.Pdf.ImageFileType.Unknown;
            // path for source barcode image file
            img.File = barcodeImagePath;
            page.Paragraphs.Add(img);
            // return updated PDF document
            return document;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
