using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Text.Json;


[Route("[Action]")]
[ApiController]
public class HomeController : Controller
{
    [HttpGet]
    public string addProduct()
    {
       
        if(User.HasClaim(c => c.Type == "permisons" && c.Value.Contains("AddProduct")))
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

        var x = User.Claims;
        //check User Hash Claims
        if (User.HasClaim(c => c.Type == "permisons" && c.Value.Contains("GetProduct") ))
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
        if(User.HasClaim(c => c.Type == "permisons" && c.Value.Contains("UpdateProduct")))
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
            if(User.HasClaim(c => c.Type == "permisons" && c.Value.Contains("DeleteProduct")))
            {
                return "Hello World";
            }
            else
            {
                return "No Permissions";
            }
    }
    
}
