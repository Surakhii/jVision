using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using jVision.Server.Download;
using jVision.Server.Data;
using Microsoft.EntityFrameworkCore;
using jVision.Server.Models;

namespace jVision.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        private readonly JvisionServerDBContext _context;
        public DownloadController(JvisionServerDBContext context)
        {
            _context = context;
        }
        [HttpGet("{file}")]
        public async Task<IActionResult> DownloadDiagram(string file)
        {
            var Boxes = _context.Boxes.Include(i => i.Services).ToList();
            switch(file)
            {
                case "diagram":
                    //return StatusCode(400);
                    return CreateDiagram(Boxes.OrderBy(i=>Version.Parse(i.Ip)).ToList());
                case "topology":
                    break;
                default:
                    return StatusCode(500);
            }
            return StatusCode(500);

        }

        private FileResult CreateDiagram(List<Box> bl)
        {
            var myExport = new CsvExport();

            List<string> config = new List<string>
            {
                "label: %component%",
                "style: shape=%shape%;fillColor=%fill%;strokeColor=%stroke%;fontSize=%fontSize%;fontColor=%font%;aspect=fixed;%type%;image=%image%;align=center;whiteSpace=wrap;html=1",
                "namespace: csvimport-",
                "connect: {\"from\":\"refs\", \"to\":\"id\",\"style\":\"opacity=0\",\"invert\":true}",
                "width: @width",
                "parentstyle: swimlane;fontStyle=1;childLayout=stackLayout;horizontal=1;startSize=46;fillColor=#3f7de0;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;fontColor=#ffffff;aspect=fixed;fontSize=14;whiteSpace=wrap;html=1",
                "parent: parent",
                "identity: identity",
                "height: @height",
                "left:%left%",
                "padding: 15",
                "ignore: id, shape, fill, stroke, refs",
                "nodespacing: 40",
                "levelspacing: 20",
                "edgespacing: 40",
                "layout: horizontalflow"
            };
            myExport.AddContent(config);

            var subnets = bl.Select(i => i.Subnet).Distinct().ToList();
            foreach (var subnet in subnets)
            {
                var subnet_boxes = bl.Where(s => s.Subnet.Equals(subnet)).ToList();
                
                var windows_boxes = bl.Where(s => s.Subnet.Equals(subnet) && s.Os != null && String.Equals(s.Os, "Windows", StringComparison.OrdinalIgnoreCase));
                var linux_boxes = bl.Where(s => s.Subnet.Equals(subnet) && s.Os != null && String.Equals(s.Os, "Linux", StringComparison.OrdinalIgnoreCase));
                var other_boxes = bl.Where(s => s.Subnet.Equals(subnet) && s.Os != null && (!String.Equals(s.Os, "Windows", StringComparison.OrdinalIgnoreCase) && !String.Equals(s.Os, "Linux", StringComparison.OrdinalIgnoreCase)));
                var subnet_record = new List<Diagram>
                {
                    new Diagram{id=$"subnet{subnet}", component=subnet, refs="", fill="#3f7de0",stroke="",shape="rectangle",type="",image="",font="#FFFFFF", fontSize="14", parent="",height="40", width="200",identity=$"subnet{subnet}"}
                };
                myExport.AddRows(subnet_record);

                foreach (var box in subnet_boxes)
                {
                    string message = "";
                    foreach (var s in box.Services.Where(s => String.Equals(s.State, "open", StringComparison.OrdinalIgnoreCase)).ToList())
                    {
                        message += "Port " + s.Port.ToString() + " - " + (s.Name ?? "") + "<br>";
                    }
                    var info_record = new List<Diagram>
                    {
                        new Diagram{id=$"info{box.BoxId}",component=(box.Ip + "<br>" + box.Hostname),refs=$"box{box.BoxId}",fill="#3f7de0",stroke="",shape="",type="swimlane",image="",width="220",height="80",font="#FFFFFF",fontSize="14",parent="", identity=$"info{box.BoxId}"}
                    };
                    var text_record = new List<Diagram>
                    {
                        new Diagram{id=$"text{box.BoxId}",component=message,refs="",fill="",stroke="",shape="",type="text",image="",width="220",height="80",font="#000000", fontSize="14",parent=$"info{box.BoxId}", identity=$"text{box.BoxId}"}
                    };

                        myExport.AddRows(info_record);
                        myExport.AddRows(text_record);
                }


                foreach (var box in windows_boxes)
                {
                    var image_record = new List<Diagram>
                {
                    new Diagram{id=$"box{box.BoxId}", component="",refs="",fill="",stroke="",shape="",type="image",image="img/lib/mscae/VirtualMachineWindows.svg",width="90",height="80",font="",fontSize="",parent="",identity=$"box{box.BoxId}"}
                };
                    myExport.AddRows(image_record);
                }
                foreach (var box in linux_boxes)
                {
                    var image_record = new List<Diagram>
                {
                    new Diagram{id=$"box{box.BoxId}", component="",refs="",fill="",stroke="",shape="",type="image",image="img/lib/mscae/VM_Linux.svg",width="90",height="80",font="",fontSize="",parent="",identity=$"box{box.BoxId}"}
                };
                    myExport.AddRows(image_record);
                }
                foreach (var box in other_boxes)
                {
                    var image_record = new List<Diagram>
                {
                    new Diagram{id=$"box{box.BoxId}", component="",refs="",fill="",stroke="",shape="",type="image",image="",width="90",height="80",font="",fontSize="",parent="",identity=$"box{box.BoxId}"}
                };
                    myExport.AddRows(image_record);
                }
            }

            //List<string> header = new List<string> { "id", "component", "refs", "fill", "stroke", "shape", "type", "image", "width", "height", "font", "fontSize", "parent", "identity" };

           




            /**
            foreach (var box in windows_boxes)
            {
                var records = new List<Diagram>
                {
                    new Diagram{Id=$"box{box.BoxId}" }
                }
                myExport.AddRows(records);
            }
            **/
            return File(myExport.ExportToBytes(), "text/csv", "diagram.csv");
        }
    }
}
