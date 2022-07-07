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
using System.Windows.Shapes;
using OverAudible.Models;

namespace OverAudible
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : Window
    {
        public Item Book
        {
            get { return (Item)GetValue(BookProperty); }
            set { SetValue(BookProperty, value); }
        }
        public static readonly DependencyProperty BookProperty =
            DependencyProperty.Register("Book", typeof(Item), typeof(Player));

        public ImageSource Media
        {
            get { return (ImageSource)GetValue(MediaProperty); }
            set { SetValue(MediaProperty, value); }
        }
        public static readonly DependencyProperty MediaProperty =
            DependencyProperty.Register("Media", typeof(ImageSource), typeof(Player));

        public string CurrentChapter
        {
            get { return (string)GetValue(CurrentChapterProperty); }
            set { SetValue(CurrentChapterProperty, value); }
        }
        public static readonly DependencyProperty CurrentChapterProperty =
            DependencyProperty.Register("CurrentChapter", typeof(string), typeof(Player), new PropertyMetadata(string.Empty));

        public TimeSpan EllapsedTime
        {
            get { return (TimeSpan)GetValue(EllapsedTimeProperty); }
            set { SetValue(EllapsedTimeProperty, value); }
        }
        public static readonly DependencyProperty EllapsedTimeProperty =
            DependencyProperty.Register("EllapsedTime", typeof(TimeSpan), typeof(Player), new PropertyMetadata(TimeSpan.FromMinutes(0)));

        public TimeSpan RemainingTime
        {
            get { return (TimeSpan)GetValue(RemainingTimeProperty); }
            set { SetValue(RemainingTimeProperty, value); }
        }
        public static readonly DependencyProperty RemainingTimeProperty =
            DependencyProperty.Register("RemainingTime", typeof(TimeSpan), typeof(Player), new PropertyMetadata(TimeSpan.FromMinutes(0)));

        public Player()
        {
            InitializeComponent();

            this.DataContext = this;
        }
    }
}
