# Welcome to WinPDF

This program was created for the purpose of editing PDF files without using a web browser in a non-networked Windows environment.

Here are some of the features we'll be introducing first, and please feel free to contact us via email below for further improvements!

> deveb1479@gmail.com

## Instruction manual

1. The screen is largely divided into three columns, and we will call them View, Load, and Tool screens, respectively.
2. The View screen is a WebView window that previews the selected PDF file, where you can preview the PDF.
3. The Load screen contains a list of PDF files from an external disk and a button to import PDFs, where you can select PDFs.
4. The selected PDF will briefly display information in the "Selected PDF Information" at the top of the Tool screen, and when you set and approve pages of the PDF in the numeric window below, a portion of the PDF will be added to the list of results below.
5. Of the three bottom buttons on the Tool screen, you can preview the merge of all results, and the Save Merge allows you to save the merge like a preview. You can also use the Save Split to save all results one by one.

## APIs

I used the following nuget package API.

1. [Microsoft.Web.WebView2 (1.0.2210.55)](https://learn.microsoft.com/en-us/microsoft-edge/webview2/)
2. [PDFsharp (6.0.0)](https://docs.pdfsharp.net/)
