using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using jVision.Server.Data;
using jVision.Shared;
using Microsoft.AspNetCore.SignalR;
using jVision.Shared.Models;
using jVision.Server.Models;

namespace jVision.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BoxController : ControllerBase
    {
        private readonly JvisionServerDBContext _context;
        //HUB CONTEXt
        public BoxController(JvisionServerDBContext context)
        {
            _context = context;

        }


        [HttpGet]

        public async Task<ActionResult<IEnumerable<BoxDTO>>> GetBox()
        {
            return await _context.Boxes
                .Select(x => BoxToDTO(x))
                .Include(s => s.Services)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<BoxDTO>>> PostBox(IEnumerable<BoxDTO> bto)
        {
            //async vs sync?
            _context.Boxes.AddRange(bto.Select(b => new Box
            {
                //UserId = b.UserId,
                Ip = b.Ip,
                Hostname = b.Hostname,
                State = b.State,
                Comments = b.Comments,
                Active = b.Active,
                Pwned = b.Pwned,
                Unrelated = b.Unrelated,
                Comeback = b.Comeback,
                Os = b.Os,
                Cidr = b.Cidr,
                Services = (ICollection<Service>)b.Services
            }));
            await _context.SaveChangesAsync();

            return StatusCode(200);
        }

        private bool BoxExists(int id)
        {
            return _context.Boxes.Any(e => e.BoxId == id);
        }
        /**

        **/

        private static BoxDTO BoxToDTO(Box box) =>
            new BoxDTO
            {
                BoxId = box.BoxId,
                UserId = box.UserId,
                Ip = box.Ip,
                Hostname = box.Hostname,
                State = box.State,
                Comments = box.Comments,
                Active = box.Active,
                Pwned = box.Pwned,
                Unrelated = box.Unrelated,
                Comeback = box.Comeback,
                Os = box.Os,
                Cidr = box.Cidr,
                Services = (ICollection<ServiceDTO>)box.Services
            };

        private static ServiceDTO ServiceToDTO(Service s) =>
            new ServiceDTO
            {
                BoxId = s.BoxId,
                ServiceId = s.ServiceId,
                Port = s.Port,
                Protocol = s.Protocol,
                State = s.State,
                Name = s.Name,
                Version = s.Version,
                Script = s.Script
            };
        public ICollection<ServiceDTO> Services { get; set; }

    }
}

