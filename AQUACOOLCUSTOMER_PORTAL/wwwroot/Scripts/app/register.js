var app = angular.module("app");

var RegistrationController = function ($http, $sce) {
    var model = this;
    model.IsBusy = true;
    model.registeredas = 'true';
    model.loadFirstBuilding = true;
    model.data = {};
    model.data.AccountType = "118290000";
    var onloadCallback = function () {

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
                    function (e) {
                        e.preventDefault();
                        alert("Use the Calendar Button to Select Date");
                    });
        $("#IdExpiryDate")
            .on("keydown",
                function (e) {
                    e.preventDefault();
                    alert("Use the Calendar Button to Select Date");
                });
    };

    model.test = function (e) {
        var a = grecaptcha.getResponse();
        if (!model.Agree) {
            alert("Please agree to the End User Agreement.");
            e.preventDefault();
            return;
        }

        if (!a) {
            alert("Are you a robot. Please select I'm not a robot");
            e.preventDefault();
            return;
        }

        //        if (a) {
        //            $http.post("https://www.google.com/recaptcha/api/siteverify",
        //                {
        //                    secret: "6LcQpAsTAAAAAEeuowS4EHFUkjI0fb9nBL2d-SUm",
        //                    response: a
        //                })
        //                .success(function (result) {
        //                    console.log(result);
        //                });
        //        }
    }

    model.init = function () {

        model.data.AccountType = "118290000";
        $http.post("/api/data/Projects", null)
            .success(function (result) {
                model.projects = result.Projects;
                model.buildings = result.Buildings;
                model.projectBuildings = model.buildings;
                //model.units = result.BuildingUnits;

                angular.forEach(model.projects,
                    function (p) {
                        if (p.Name == 'Al Faraa - Manhattan') {
                            model.data.project = p;
                            model.projectSelect();
                        }
                    });

                angular.forEach(model.buildings,
                    function (b) {
                        if (b.Name == 'Manhattan Tower') {
                            model.data.building = b;
                            model.buildingSelect();
                            model.IsBusy = false;

                        }
                    });
            });
    };

    model.projectSelect = function () {
        console.log("Project Changed");

        if (model.data.project) {

            model.projectBuildings = $.grep(model.buildings,
                function (value, index) {
                    return model.data.project.ProjectId == value.ProjectId;
                });

        } else {
            model.data.project = {};
            model.data.building = {};
            model.data.unit = null;
            //alert("Please select a project");
        }
    }

    model.buildingSelect = function () {
        if (model.data.building) {
            if (!model.loadFirstBuilding) {
                $("#unitId").select2("destroy");
            }

            $http.post("/api/data/BuildingUnits?projectId=" +
                    model.data.project.ProjectId +
                    "&AccountType=" +
                    model.data.AccountType)
                .success(function (result) {
                    model.buildingUnits = result;

                    model.selectedBuildingUnits = $.grep(model.buildingUnits,
                        function(value, index) {
                            return model.data.building.BuildingId == value.BuildingId;
                        });

                    model.IsBusy = false;
                    setTimeout(function () {
                        $("#unitId").select2();
                        model.loadFirstBuilding = false;
                    }, 1000);
                });

        } else {
            model.data.building = {};
            model.data.unit = null;
            alert("Please select a building");
        }

        //        model.buildingUnits = $.grep(model.units,
        //            function (value, index) {
        //                return model.data.building.BuildingId == value.BuildingId;
        //            });
    }

    model.accountTypeSelect = function () {
        model.data.unit = null;
        model.IsBusy = true;

        model.buildingSelect();
    }

    model.unitSelect = function () {
        console.log(model.data.unit);
        if (model.data.unit) {
            model.data.UnitTypeName = model.data.unit.UnitTypeName;
            setTimeout(onloadCallback, 1000);
        }

        //model.EUA = $sce.trustAsResourceUrl("/ViewerJS/?zoom=page-width#../Files/" + model.data.project.EUA);

        model.EUA = $sce.trustAsResourceUrl("../Files/" + model.data.project.EUA);

        //$("#iFrame")
        //    .innerHTML = "<iframe src='/ViewerJS/?zoom=page-width#../Files/" +
        //        model.data.project.EUA +
        //        "' width='100%' height='500' allowfullscreen webkitallowfullscreen></iframe>";
    }
}
app.controller("RegistrationController", RegistrationController);


