let currentCustStep = 1; // Renamed to currentCustStep
let updateCustProgressBar; // Renamed to updateCustProgressBar

document.addEventListener("DOMContentLoaded", function () {
    // Hide all steps except the first one
    document.querySelectorAll('.custstep').forEach((step, index) => {
        if (index > 0) {
            step.style.display = "none";
        }
    });

    // Handle "Next" button click
    document.querySelectorAll(".cust-next-step").forEach(nextStepButton => {

        nextStepButton.addEventListener("click", function () {

            if (currentCustStep < 5) {

                document.querySelector(".custstep-" + currentCustStep).classList.add("animate__animated", "animate__fadeOutLeft");
                currentCustStep++;
                setTimeout(function () {
                    document.querySelectorAll(".custstep").forEach(step => {

                        step.classList.remove("animate__animated", "animate__fadeOutLeft");
                        step.style.display = "none";
                    });


                    document.querySelector(".custstep-" + currentCustStep).style.display = "block";
                    document.querySelector(".custstep-" + currentCustStep).classList.add("animate__animated", "animate__fadeInRight");
  
                }, 500);
            }
        });
    });

    // Handle "Previous" button click
    document.querySelectorAll(".cust-prev-step").forEach(prevStepButton => {
        prevStepButton.addEventListener("click", function () {
            if (currentCustStep > 1) {
                document.querySelector(".custstep-" + currentCustStep).classList.add("animate__animated", "animate__fadeOutRight");
                currentCustStep--;
                setTimeout(function () {
                    document.querySelectorAll(".custstep").forEach(step => {
                        step.classList.remove("animate__animated", "animate__fadeOutRight");
                        step.style.display = "none";
                    });
                    document.querySelector(".custstep-" + currentCustStep).style.display = "block";
                    document.querySelector(".custstep-" + currentCustStep).classList.add("animate__animated", "animate__fadeInLeft");
                }, 500);
            }
        });
    });

});
