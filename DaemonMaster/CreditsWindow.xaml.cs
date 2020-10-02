/////////////////////////////////////////////////////////////////////////////////////////
//  DaemonMaster: CreditsWindow
//  
//  This file is part of DeamonMaster.
// 
//  DeamonMaster is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//   DeamonMaster is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with DeamonMaster.  If not, see <http://www.gnu.org/licenses/>.
/////////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Windows;

namespace DaemonMaster
{
    /// <summary>
    /// Interaktionslogik für CreditsWindow.xaml
    /// </summary>
    public partial class CreditsWindow : Window
    {
        public CreditsWindow()
        {
            InitializeComponent();

            LabelCredits.Content =
                "The program \"DeamonMaster\" was originally created by:\n" +
                "   - MCPC10 (code, GUI, translations, etc)\n" +
                "   - Stuffi3000 (GUI, translations, etc)  \n\n" +
                "Used librarys: \n" +
                "   => Newtonsoft.Json - James Newton - King - MIT License \n" +
                "   => NLog - Jaroslaw Kowalski, Kim Christensen, Julian Verdurmen - BSD 3 clause \"New\" or \"Revised\" License \n" +
                "   => AutoUpdater.NET - RBSoft - MIT License \n" +
                "   => Active Directory Object Picker - Tulpep - MS-PL License \n" +
                "   => ListView Layout Manager - Jani Giannoudis - CPOL License \n" +
                "   => CommandLineParser - Giacomo Stelluti Scala & Contributors - MIT License \n" +
                "   => DotNetProjects.Extended.Wpf.Toolkit - MS-PL License \n\n" +
                "Also thanks to: \n" +
                "   - PInvoke.net \n" +
                "   - stackoverflow.com (for help from the users) \n" +
                "   - entwickler-ecke.de (for help from the users)";

            LabelVersion.Content = "v" + Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}
