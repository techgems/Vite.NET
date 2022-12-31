using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViteDotNet;

public class IntegrationConfigModel
{
    public string Entrypoint { get; set; } = string.Empty;

    public bool IsReact { get; set; }

    public int Port { get; set; } = 5173;

    public string RootDirectory { get; set; } = "ClientApp";

    public string ContainerElementId { get; set; } = "app";
}
