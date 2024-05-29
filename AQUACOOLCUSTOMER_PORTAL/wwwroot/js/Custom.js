let currentStep = 1;
let updateProgressBar;

function displayStep(stepNumber) {
    if (stepNumber >= 1 && stepNumber <= 3) {
        document.querySelector(".step-" + currentStep).style.display = "none";
        document.querySelector(".step-" + stepNumber).style.display = "block";
        currentStep = stepNumber;
        updateProgressBar();
    }
}

document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll('.step').forEach((step, index) => {
        if (index > 0) {
            step.style.display = "none";
        }
    });

    document.querySelectorAll(".next-step").forEach(nextStepButton => {
        nextStepButton.addEventListener("click", function () {
            if (currentStep < 3) {
                document.querySelector(".step-" + currentStep).classList.add("animate__animated", "animate__fadeOutLeft");
                currentStep++;
                setTimeout(function () {
                    document.querySelectorAll(".step").forEach(step => {
                        step.classList.remove("animate__animated", "animate__fadeOutLeft");
                        step.style.display = "none";
                    });
                    document.querySelector(".step-" + currentStep).style.display = "block";
                    document.querySelector(".step-" + currentStep).classList.add("animate__animated", "animate__fadeInRight");
                    updateProgressBar();
                }, 500);
            }
        });
    });

    document.querySelectorAll(".prev-step").forEach(prevStepButton => {
        prevStepButton.addEventListener("click", function () {
            if (currentStep > 1) {
                document.querySelector(".step-" + currentStep).classList.add("animate__animated", "animate__fadeOutRight");
                currentStep--;
                setTimeout(function () {
                    document.querySelectorAll(".step").forEach(step => {
                        step.classList.remove("animate__animated", "animate__fadeOutRight");
                        step.style.display = "none";
                    });
                    document.querySelector(".step-" + currentStep).style.display = "block";
                    document.querySelector(".step-" + currentStep).classList.add("animate__animated", "animate__fadeInLeft");
                    updateProgressBar();
                }, 500);
            }
        });
    });

    updateProgressBar = function () {
        const progressPercentage = ((currentStep - 1) / 2) * 100;
        document.querySelector(".progress-bar").style.width = progressPercentage + "%";
    }
});
