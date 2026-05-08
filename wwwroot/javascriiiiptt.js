window.downloadDivAsPdf = (divId, filename) => {
    const { jsPDF } = window.jspdf;
    const pdf = new jsPDF();
    const element = document.getElementById(divId);

    if (element) {
        pdf.html(element, {
            callback: function (pdf) {
                pdf.save(filename);
            },
            x: 10,
            y: 10
        });
    } else {
        console.error("Element with specified ID not found");
    }
}