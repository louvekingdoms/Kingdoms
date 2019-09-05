using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameLogger;

public class Subject
{
    public readonly int id;
    public Image image;

    public Subject(int id, string imageURL)
    {
        this.id = id;
        image = new Image(imageURL);
    }
}
