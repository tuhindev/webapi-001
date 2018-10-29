using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using WebApplication3.Data;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ITitemQuery _titemQuery;
        public TestController(ITitemQuery titemQuery)
        {
            _titemQuery = titemQuery;
        }


        public IActionResult Save(TItem item)
        {
            return Ok();
        }

        [Route("testdata")]
        [HttpGet]
        public async Task<IActionResult> TestData()
        {
            await Task.Delay(1);
            return Ok();
        }

        [Route("get/id={id}")]
        [HttpGet]
        [ProducesResponseType(typeof(TitemData), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var resp = await _titemQuery.GetSingle(id);
            return Ok(resp);
        }
    }

}