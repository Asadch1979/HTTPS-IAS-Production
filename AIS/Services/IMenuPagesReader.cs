using System.Collections.Generic;
using AIS.Models;

namespace AIS.Services
    {
    public interface IMenuPagesReader
        {
        List<MenuPagesModel> GetActiveMenuPages();
        }
    }
