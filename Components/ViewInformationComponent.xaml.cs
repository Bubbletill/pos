﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BT_POS.Components;

public partial class ViewInformationComponent : UserControl
{

    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(ViewInformationComponent), new PropertyMetadata(default(string)));



    public string Information
    {
        get { return (string)GetValue(InformationProperty); }
        set { SetValue(InformationProperty, value); }
    }

    public static readonly DependencyProperty InformationProperty =
        DependencyProperty.Register("Information", typeof(string), typeof(ViewInformationComponent), new PropertyMetadata(default(string)));


    public ViewInformationComponent()
    {
        InitializeComponent();
        POSController controller = App.AppHost.Services.GetRequiredService<POSController>();
        if (controller.TrainingMode)
        {
            MainBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFC8ABFF");
        }
    }

    public void SetAdminColour()
    {
        MainBox.Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xe0, 0xe0));
    }
}
