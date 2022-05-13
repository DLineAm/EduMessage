namespace EduMessage.ViewModels
{
    public struct Crumb
#pragma warning restore CS0659 // "Crumb" переопределяет Object.Equals(object o), но не переопределяет Object.GetHashCode().
    {
        public string Title { get; set; }
        public object Data { get; set; }

        public Crumb(string title, object data)
        {
            Title = title;
            Data = data;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || obj is not Crumb crumb)
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            if (crumb.Title == Title && crumb.Data == Data)
            {
                return true;
            }
            return false;
        }
    }
}