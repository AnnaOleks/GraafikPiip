using GraafikPiip.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraafikPiip.Services
{
    public class NavigationService : INavigationService
    {
        public Task GoToAsync(string route) => Shell.Current.GoToAsync(route);
    }
}
