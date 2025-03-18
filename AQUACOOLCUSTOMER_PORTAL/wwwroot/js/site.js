//// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
//// for details on configuring this project to bundle and minify static web assets.

//// Write your JavaScript code.

//    $(document).ready(function () {
//        $('#projectDropdown').change(function () {
//            var selectedProject = $(this).val();

//            // If no project is selected, clear the Unit Numbers dropdown
//            if (!selectedProject) {
//                $('#unitNumbersDropdown').empty().append('<option value="">Select Unit Number</option>');
//                $('#pdfViewer').hide();
//                return;
//            }

//            // Make an AJAX request to the controller to get the Unit Numbers
//            $.ajax({
//                url: '/Admin/LoadUnitSelection', // Update this to match your actual controller and action
//                type: 'GET',
//                data: { propertyId: selectedProject }, // Pass the selected project ID
//                success: function (response) {
//                    // Clear the Unit Numbers dropdown
//                    $('#unitNumbersDropdown').empty().append('<option value="">Select Unit Number</option>');

//                    // Ensure response is parsed as JSON
//                    var data = typeof response === "string" ? JSON.parse(response) : response;

//                    // Populate the Unit Numbers dropdown with the response data
//                    $.each(data, function (index, unit) {
//                        $('#unitNumbersDropdown').append('<option value="' + unit.PropertyID + '">' + unit.Unit + '</option>');
//                    });

//                    // Load the PDF file corresponding to the selected project
//                    var pdfFilePath = '/EUA/MAG-000001.pdf'; // Update with correct file path
//                   // var pdfFilePath = '/EUA/MAG-000001.pdf' + selectedProject + '.pdf'; // Update with correct file path
//                    $('#pdfViewer').attr('src', pdfFilePath);
//                    $('#pdfViewer').show(); // Show the iframe

//                    // Set the download button with the correct link
//                    $('#downloadButton').attr('href', pdfFilePath).attr('download', 'Agreement' + '.pdf');
//                    $('#downloadButton').show(); // Show the download button
//                },
//                error: function (xhr, status, error) {
//                    console.error('Error fetching unit numbers:', error);
//                }
//            });
//        });
//    });
$(document).ready(function () {
    $('#projectDropdown').change(function () {
        var selectedProject = $(this).val();

        // If no project is selected, clear the Unit Numbers dropdown
        if (!selectedProject) {
            $('#unitNumbersDropdown').empty().append('<option value="">Select Unit Number</option>');
            $('#pdfViewer').hide();
            return;
        }

        // Make an AJAX request to get the Unit Numbers
        $.ajax({
            url: '/Admin/LoadUnitSelection',
            type: 'GET',
            data: { propertyId: selectedProject },
            success: function (response) {
                $('#unitNumbersDropdown').empty().append('<option value="">Select Unit Number</option>');

                var data = typeof response === "string" ? JSON.parse(response) : response;

                $.each(data, function (index, unit) {
                    $('#unitNumbersDropdown').append('<option value="' + unit.PropertyID + '">' + unit.Unit + '</option>');
                });

                var pdfFilePath = '/EUA/MAG-000001.pdf';
                $('#pdfViewer').attr('src', pdfFilePath).show();
                $('#downloadButton').attr('href', pdfFilePath).attr('download', 'Agreement.pdf').show();
                // set the declare load

                //var selectedUnit = $(this).val();

                //if (!selectedUnit) {
                //    $('#lblSecurityDeposit').text(''); // Clear label if no unit is selected
                //    return;
                //}
                console.log("selected projects "+selectedProject);
                $.ajax({
                    url: '/api/data2/AllocatedSDAmt?property=' + selectedProject, // Update with the correct controller/action
                    type: 'POST',
                    data: { property: selectedProject }, // Send selected unit ID
                    success: function (response) {
                        //var securityDeposit = response.securityDeposit || 'N/A'; // Fallback if no data
                        var securityDeposit = response['sdAmount']; // Fallback if no data
                        $('#lblSecurityDeposit').text(' ' + securityDeposit);
                    },
                    error: function (xhr, status, error) {
                        console.error('Error fetching security deposit:', error);
                        $('#lblSecurityDeposit').text('Error fetching data');
                    }
                });

                // Get declared load 
                $.ajax({
                    url: '/api/data2/UnitDL?property=' + selectedProject, // Update with the correct controller/action
                    type: 'POST',
                    data: { property: selectedProject }, // Send selected unit ID
                    success: function (response) {
                        //var securityDeposit = response.securityDeposit || 'N/A'; // Fallback if no data
                        var declareLoad = response['unitdl']; // Fallback if no data
                        $('#lblDeclareLoad').text(' ' + declareLoad);
                    },
                    error: function (xhr, status, error) {
                        console.error('Error fetching security deposit:', error);
                        $('#lblDeclareLoad').text('Error fetching data');
                    }
                });

            },
            error: function (xhr, status, error) {
                console.error('Error fetching unit numbers:', error);
            }
        });
    });

    
});
