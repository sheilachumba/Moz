// Function to download a file from a base64 string
function downloadFromBase64(fileName, base64Data, contentType) {
    // Convert base64 to binary
    const binaryString = window.atob(base64Data);
    const bytes = new Uint8Array(binaryString.length);
    
    for (let i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i);
    }
    
    // Create a Blob from the binary data
    const blob = new Blob([bytes], { type: contentType });
    
    // Create a download link and trigger the download
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    
    // Clean up
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
}

// Make the function available globally
window.downloadFromBase64 = downloadFromBase64;
