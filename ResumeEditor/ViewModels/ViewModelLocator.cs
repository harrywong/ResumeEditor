using Autofac.Extras.CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Autofac;
using Microsoft.Practices.ServiceLocation;

namespace ResumeEditor.ViewModel
{
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MainViewModel>();

            var container = builder.Build();
            var locator = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => locator);
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}