﻿namespace Lab1Try2.Config;
using System.Text;
using System.Text.Json;

public class RabbitMqSettings
{
    public string HostName { get; set; }
    public int Port { get; set; }

    public string OrderCreatedQueue { get; set; }
}
