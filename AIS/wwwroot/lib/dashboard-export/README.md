Pinned browser distributions, served locally under the existing CSP:

- html2canvas 1.4.1: https://cdn.jsdelivr.net/npm/html2canvas@1.4.1/dist/html2canvas.min.js
- jsPDF 4.2.1: https://cdn.jsdelivr.net/npm/jspdf@4.2.1/dist/jspdf.umd.min.js
- ExcelJS 4.4.0: https://cdn.jsdelivr.net/npm/exceljs@4.4.0/dist/exceljs.min.js

Upstream MIT licenses are included. Dashboard exports make no network data requests.
Only PNG images created by the section renderer are passed to jsPDF. Excel table
strings are literal values, never formula objects. No user-provided HTML is parsed
by the export code.
