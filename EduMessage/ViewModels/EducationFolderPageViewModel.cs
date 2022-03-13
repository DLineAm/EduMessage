﻿using EduMessage.Services;

using MvvmGen;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class EducationFolderPageViewModel
    {
        [Property] private List<Speciality> _specialities;
        [PropertyCallMethod(nameof(Navigate))]
        [Property] private Speciality _speciality;

        public async Task Initialize()
        {
            try
            {
                var response = (await (App.Address + "Home/Specialities")
                    .GetStringAsync())
                    .DeserializeJson<List<Speciality>>();

                Specialities = response;
            }
            catch (Exception e)
            {

                
            }
        }

        private void Navigate()
        {
            App.InvokeSelectedSpecialityChanged(Speciality);
        }
    }
}