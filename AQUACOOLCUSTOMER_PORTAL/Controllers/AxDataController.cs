using System;
using System.Collections.Generic;
using System.Linq;
using AQUACOOLCUSTOMER_PORTAL.Controllers;
using Microsoft.AspNetCore.Mvc;
using ServiceReference1;

namespace Aquacool.Web.Controllers
{
    [Route("api/data2")]
    [ApiController]
    public class AxDataController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        private ServiceReference1.Service1SoapClient _service;
        //private readonly CustomerServiceClient _service;

        public AxDataController(ILogger<AdminController> logger)
        {
            _logger = logger;
            _service = new Service1SoapClient(Service1SoapClient.EndpointConfiguration.Service1Soap);
        }

        [Route("Test")]
        [HttpGet]
        public string Test()
        {
            return "working.";
        }

        [Route("Projects")]
        [HttpPost]
        public object Projects()
        {
            try
            {
                Request.Headers.Add("accept", "application/json");
                var projects = _service.GetProjectsAsync().Result.ToList();
                return new { Projects = projects };
            }
            catch (Exception ex)
            {
                throw ex;
            }
            // BuildingUnits = _service.GetUnits()
        }


        [Route("Units")]
        [HttpPost]
        public object Units(string project)
        {
            try
            {
                Request.Headers.Add("accept", "application/json");
                var Units = _service.getProjectDetailsAllAsync(project).Result.ToList();
                return new { Units };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Route("Property")]
        [HttpPost]
        public object Property(string property)
        {
            return new { Property = _service.getPropertyDetailsAsync(property).Result };
        }

        [Route("Countries")]
        [HttpGet]
        public object Countries()
        {
            try
            {
                Request.Headers.Add("accept", "application/json");
                var d = _service.GetCountriesAsync().Result.ToList();
                return new { Countries = d };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [Route("Cities")]
        [HttpGet]
        public object Cities(string countryId)
        {
            try
            {
                Request.Headers.Add("accept", "application/json");
                var d = _service.GetCitiesAsync(countryId).Result.ToList();
                return new { Cities = d, Buildings = d };
            }
            catch (Exception ex)
            {
                throw ex;
            }
            // BuildingUnits = _service.GetUnits()
        }

        [Route("AllocatedSDAmt")]
        [HttpPost]
        public object AllocatedSDAmt(string property)
        {
            var allocatedSDAmt = _service.GetAllocatedSDAmtAsync(property).Result;
            return new
            {
                SDAmount = allocatedSDAmt
            };
        }

        [Route("UnitDL")]
        [HttpPost]
        public object UnitDL(string property)
        {
            var unitDL = _service.GetUnitDLAsync(property).Result;
            return new { unitdl = unitDL };
        }
    }
}

