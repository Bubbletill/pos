using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace BT_POS.Components;

public class ActionableContentPresenter : ContentPresenter
{
    static ActionableContentPresenter()
    {
        ContentProperty.OverrideMetadata(typeof(ActionableContentPresenter),
         new FrameworkPropertyMetadata(new PropertyChangedCallback(OnContentChanged)));
    }

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var mcc = d as ActionableContentPresenter;
        mcc?.ContentChanged?.Invoke(mcc,
              new DependencyPropertyChangedEventArgs(ContentProperty, e.OldValue, e.NewValue));
    }
    public event DependencyPropertyChangedEventHandler ContentChanged;
}
