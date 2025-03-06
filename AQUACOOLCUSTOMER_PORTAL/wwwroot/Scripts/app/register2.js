var app = angular.module("app");

var RegistrationController = function($http, $sce) {
    var model = this;
    model.data = {};

    var onloadCallback = function() {
        console.log("on callback called");
        console.log(window.customer);
        var inner = $("#recaptchaIn");
        inner.html("");
        inner.append("<div id='recaptcha'></div>");

        var c = $("#recaptcha");
        grecaptcha.render('recaptcha',
            {
                'sitekey': '6LcQpAsTAAAAAEeuowS4EHFUkjI0fb9nBL2d-SUm'
            });

        $("#PassportExpiryDate")
            .on("keydown",
                function(e) {
                    e.preventDefault();
                    alert("Use the Calendar Button to Select Date");
                });
        $("#EmailAddress1")
            .on("onblur",
                function (e) {
                    e.preventDefault();
                    alert("Use the Calendar Button to Select Date");
                });

        $("#IdExpiryDate")
            .on("keydown",
                function(e) {
                    e.preventDefault();
                    alert("Use the Calendar Button to Select Date");
                });
    };

    model.test = function(e) {
        var a = grecaptcha.getResponse();
        console.log(model.data);
        //e.preventDefault();
        //return;

        if (model.data.Resident === "Resident") {
            if (!model.data.IdNumber) {
                alert("Emirates Id is required for Residents. Please provide one");
                e.preventDefault();
                return;
            }
        }

        if (!model.data.Agreement) {
            alert("Please confirm the disclaimer.");
            e.preventDefault();
            return;
        }

        if (!a) {
            alert("Are you a robot. Please select I'm not a robot");
            e.preventDefault();
            return;
        }
    };

    model.update = function (e) {
        var a = grecaptcha.getResponse();
        console.log(model.data);
        //e.preventDefault();
        //return;

        if (!a) {
            alert("Are you a robot. Please select I'm not a robot");
            e.preventDefault();
            return;
        }
    };

    model.init = function() {
        setTimeout(onloadCallback, 1000);

        if (window.customer) {
            model.data = window.customer;
        }
    };

};
app.controller("RegistrationController", RegistrationController);


