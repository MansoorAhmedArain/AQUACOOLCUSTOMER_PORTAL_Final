var app = angular.module("app");

var RegistrationController = function ($http, $sce) {
    var model = this;
    model.IsBusy = true;
    model.data = {};
    model.data.AccountType = "";

    model.init = function () {
        //model.data.AccountType = "118290000";
        $http.post("/api/data2/Projects", null)
            .success(function (result) {
                model.projects = result.Projects;
                //model.units = result.BuildingUnits;

                angular.forEach(model.projects,
                    function (p) {
                        if (p.Name == 'Al Faraa - Manhattan') {
                            model.data.project = p;
                            model.projectSelect();
                        }
                    });
            });
    };

    model.projectSelect = function () {
        console.log("Project Changed");

        $http.post("/api/data2/Units?project=" + model.data.project.idField)
            .success(function (result) {
                console.log(result);
                model.units = result.Units;
            });

        //model.data.project = {};
        //model.data.unit = null;
    }

    model.unitSelect = function () {
        console.log(model.data.unit);

        $http.post("/api/data2/Property?property=" + model.data.unit.propertyIDField)
            .success(function (result) {
                //model.unitid = result.propertyIDField;
                
                //document.getElementById("lblSecurityDeposit").innerHTML = model.data.unit.propertyIDField + " -*- " + model.data.unit.unitField
                //document.getElementById("lblDeclareLoad").innerHTML = model.data.unit.propertyIDField + " -*- " + model.data.unit.unitField

                //console.log("hellowwww" + model.data.unit.propertyIDField);
                console.log(result);
            });

        $http.post("/api/data2/AllocatedSDAmt?property=" + model.data.unit.propertyIDField)
            .success(function (result) {

                document.getElementById("lblSecurityDeposit").innerHTML = result.SDAmount;
                console.log(result.SDAmount);
            });
        $http.post("/api/data2/UnitDL?property=" + model.data.unit.propertyIDField)
            .success(function (result) {
                console.log(result.unitdl);

                document.getElementById("lblDeclareLoad").innerHTML = result.unitdl;
            });
        //if (model.data.unit) {
        //    model.data.UnitTypeName = model.data.unit.UnitTypeName;
        //    setTimeout(onloadCallback, 1000);
        //}

        //model.EUA = $sce.trustAsResourceUrl("../Files/" + model.data.project.EUA);

        //$("#iFrame")
        //    .innerHTML = "<iframe src='/ViewerJS/?zoom=page-width#../Files/" +
        //        model.data.project.EUA +
        //        "' width='100%' height='500' allowfullscreen webkitallowfullscreen></iframe>";
    };

    model.accountTypeSelect = function () {
        model.data.unit = null;
        model.IsBusy = true;
    };


};
app.controller("RegistrationController", RegistrationController);


