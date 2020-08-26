function FileSaveAs(filename, fileContent) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:text/plain;charset=utf-8," + encodeURIComponent(fileContent)
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function ReadUploadedFileAsText(inputFile) {
    var input = document.getElementById(inputFile);
    var file = input.files[0];

    var fr = new FileReader();

    return new Promise((resolve, reject) => {
        fr.onerror = () => {
            fr.abort();
            console.log("rejected promise method")
            reject(new DOMException("Problem parsing input file."));
        };

        fr.onload = () => {
            console.log(fr.result)
            resolve({
                filename: file.name,
                data: fr.result,
            });
        };
        fr.readAsText(file);
    });
};