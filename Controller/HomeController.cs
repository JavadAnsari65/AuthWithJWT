using Microsoft.AspNetCore.Mvc;

//route ["Action"] is the name of the method

[Route("[Action]")]

//apicontroller
[ApiController]

public class HomeController : Controller
{

    [HttpGet]

    public string addProduct()
    {

        //check User Hash Claims "permisons": [
//     "addproduct",
//     "deleteproduct",
//     "updateproduct",
//     "getproduct"
//   ],
       
        if(User.HasClaim(c => c.Type == "permisons" && c.Value.Contains("addproduct")))
        {
            return "Hello World";
        }
        else
        {
            return "No Permissions";
        }
       
       
    }


    [HttpGet]
    public string getProduct()
    {
         //check User Hash Claims
        if(User.HasClaim(c => c.Type == "permisons" && c.Value.Contains( "getproduct") ))
        {
            return "Hello World";
        }
        else
        {
            return "No Permissions";
        }
        
    }

    [HttpGet]
    public string updateProduct()
    {
         //check User Hash Claims
        if(User.HasClaim(c => c.Type == "permisons" && c.Value.Contains("updateproduct")))
        {
            return "Hello World";
        }
        else
        {
            return "No Permissions";
        }
    }

    [HttpGet]
    public string deleteProduct()
    {
            //check User Hash Claims
            if(User.HasClaim(c => c.Type == "permisons" && c.Value.Contains("deleteproduct")))
            {
                return "Hello World";
            }
            else
            {
                return "No Permissions";
            }
    }
    
}
