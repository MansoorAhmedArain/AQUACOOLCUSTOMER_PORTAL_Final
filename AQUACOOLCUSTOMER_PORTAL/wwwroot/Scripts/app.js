
var wrapper = document.getElementById("signature-pad");
var clearButton = wrapper.querySelector("[data-action=clear]"),
    saveButton = wrapper.querySelector("[data-action=save]"),
    canvas = wrapper.querySelector("canvas"),
    signaturePad;

// Adjust canvas coordinate space taking into account pixel ratio,
// to make it look crisp on mobile devices.
// This also causes canvas to be cleared.
function resizeCanvas() {
    // When zoomed out to less than 100%, for some very strange reason,
    // some browsers report devicePixelRatio as less than 1
    // and only part of the canvas is cleared then.
    var ratio = Math.max(window.devicePixelRatio || 1, 1);
    canvas.width = canvas.offsetWidth * ratio;
    canvas.height = canvas.offsetHeight * ratio;
    canvas.getContext("2d").scale(ratio, ratio);
}

window.onresize = resizeCanvas;
resizeCanvas();

signaturePad = new SignaturePad(canvas);

clearButton.addEventListener("click", function (event) {
    signaturePad.clear();
});

//saveButton.addEventListener("click", function (event) {
//    if (signaturePad.isEmpty()) {
//        alert("Please provide signature ! Signature pad is empty.");
//    } else {       
//
//      //  window.open(signaturePad.toDataURL());
//        var sigImg = signaturePad.toDataURL();
//        var myBaseString = sigImg;
//        var block = myBaseString.split(";");
//        var dataType = block[0].split(":")[1];
//        var realData = block[1].split(",")[1];
//        var folderpath = "D:/";
//        // The name of your file, note that you need to know if is .png,.jpeg etc
//        var filename = "signature.png";
//
//        alert(realData);
//        alert(dataType);
//        alert(folderpath);
//
//        download(realData, filename, dataType);
//        
//    }
//});


function download(strData, strFileName, strMimeType) {
    var doc = document, arg = arguments, a = doc.createElement("a"),
        d = arg[0], n = arg[1], t = arg[2] || "application/pdf";

    var newdata = "data:" + strMimeType + ";base64," + escape(strData);

    //build download link:
    a.href = newdata;

    if ('download' in a) {
        a.setAttribute("download", n);
        a.innerHTML = "downloading...";
        doc.body.appendChild(a);
        setTimeout(function () {
            var e = doc.createEvent("MouseEvents");

            e.initMouseEvent("click", true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null
            );
            a.dispatchEvent(e);
            doc.body.removeChild(a);
        }, 66);
        return true;
    };
}

