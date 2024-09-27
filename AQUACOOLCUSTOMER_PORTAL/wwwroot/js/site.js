// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

    $(document).ready(function () {
        $('#projectDropdown').change(function () {
            var selectedProject = $(this).val();

            // If no project is selected, clear the Unit Numbers dropdown
            if (!selectedProject) {
                $('#unitNumbersDropdown').empty().append('<option value="">Select Unit Number</option>');
                $('#pdfViewer').hide();
                return;
            }

            // Make an AJAX request to the controller to get the Unit Numbers
            $.ajax({
                url: '/Admin/LoadUnitSelection', // Update this to match your actual controller and action
                type: 'GET',
                data: { propertyId: selectedProject }, // Pass the selected project ID
                success: function (response) {
                    // Clear the Unit Numbers dropdown
                    $('#unitNumbersDropdown').empty().append('<option value="">Select Unit Number</option>');

                    // Ensure response is parsed as JSON
                    var data = typeof response === "string" ? JSON.parse(response) : response;

                    // Populate the Unit Numbers dropdown with the response data
                    $.each(data, function (index, unit) {
                        $('#unitNumbersDropdown').append('<option value="' + unit.PropertyID + '">' + unit.Unit + '</option>');
                    });

                    // Load the PDF file corresponding to the selected project
                    var pdfFilePath = '/EUA/MAG-000001.pdf'; // Update with correct file path
                   // var pdfFilePath = '/EUA/MAG-000001.pdf' + selectedProject + '.pdf'; // Update with correct file path
                    $('#pdfViewer').attr('src', pdfFilePath);
                    $('#pdfViewer').show(); // Show the iframe

                    // Set the download button with the correct link
                    $('#downloadButton').attr('href', pdfFilePath).attr('download', 'Agreement' + '.pdf');
                    $('#downloadButton').show(); // Show the download button
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching unit numbers:', error);
                }
            });
        });
    });
