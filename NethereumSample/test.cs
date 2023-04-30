using System;
using Xamarin.Forms;

namespace XamarinExample
{
    public partial class MainPage : ContentPage
    {
       int count = 0;
        void Button_Clicked(object sender, System.EventArgs e)
        {
            count++;
            ((Button)sender).Text = $"You clicked {count} times.";
        }
    }
}