using System;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;
using System.Xml;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ODataUntypedSample
{
    public class ODataUntypedSample
    {
		public static IEdmModel Model = GetEdmModel();

		public static void Main(string[] args)
		{
			using (WebApp.Start(ServiceUrl, Configuration))
			{
				Console.WriteLine("Server is listening at {0}", ServiceUrl);
				RunSample();
				Console.WriteLine("Press any key to continue . . .");
				Console.ReadKey();
			}
		}
		
		public static void RunSample()
		{
			HttpResponseMessage response = client.GetAsync(ServiceUrl + "/odata/$metadata").Result; PrintResponse(response);
			HttpResponseMessage response1 = client.GetAsync(ServiceUrl + "/odata/Customers").Result; PrintResponse(response1);
			HttpResponseMessage response4 = client.GetAsync(ServiceUrl + "/odata/Customers?$expand=Products").Result; PrintResponse(response4);
		}

		public static IEdmModel GetEdmModel()
		{
			EdmModel model = new EdmModel();

			#region Building Customer schema

			var customerType = new EdmEntityType("NS", "Customer");
			var customerIdProperty =
			customerType.AddStructuralProperty("CustomerId", EdmPrimitiveTypeKind.Guid);
			customerType.AddStructuralProperty("FullName", EdmPrimitiveTypeKind.String);
			customerType.AddKeys(new IEdmStructuralProperty[] { customerIdProperty });
			model.AddElement(customerType);

			#endregion

			#region Building Product schema

			var productType = new EdmEntityType("NS", "Product");
			var productIdProperty = productType.AddStructuralProperty("ProductId", EdmPrimitiveTypeKind.Guid);
			var productCustomerId = productType.AddStructuralProperty("CustomerIdRef", EdmPrimitiveTypeKind.Guid);
			productType.AddKeys(productIdProperty);
			model.AddElement(productType);

			#endregion

			#region Creating 1:N relationship (Customer -> Product)
			var productGrid = new EdmNavigationPropertyInfo
			{
				ContainsTarget = false,
				Name = "Products",
				Target = productType,
				TargetMultiplicity = EdmMultiplicity.Many,
			};
			var CoustomersProduct = customerType.AddUnidirectionalNavigation(productGrid);
			#endregion

			#region Customer ref field
			var CustomerLookup = productType.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
			{
				ContainsTarget = false,
				Name = "Customer",
				Target = customerType,
				TargetMultiplicity = EdmMultiplicity.One,
				DependentProperties = new[] { productCustomerId },
				PrincipalProperties = new[] { customerIdProperty }
			});
			#endregion

			// Create and add entity container.
			EdmEntityContainer container = new EdmEntityContainer("NS", "DefaultContainer");
			model.AddElement(container);

			EdmEntitySet Customers = container.AddEntitySet("Customers", customerType);
			EdmEntitySet Products = container.AddEntitySet("Products", productType);
			Customers.AddNavigationTarget(CoustomersProduct, Products);
			Products.AddNavigationTarget(CustomerLookup, Customers);

			return model;
		}

		private static HttpClient client = new HttpClient();
		private const string ServiceUrl = "http://localhost:12345";

		public static void Configuration(IAppBuilder builder)
        {
            HttpConfiguration configuration = new HttpConfiguration();
			configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
			configuration.MapODataServiceRoute("odata", "odata", Model);
            builder.UseWebApi(configuration);
        }
		
        public static void PrintResponse(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Response:");
            Console.WriteLine(response);

#if WEBAPIODATA_6X
            if (response.Content != null)
            {
                string payload = response.Content.ReadAsStringAsync().Result;

                if (response.Content.Headers.ContentType.MediaType.Contains("xml"))
                {
                    Console.WriteLine(FormatXml(payload));
                }
                else if (response.Content.Headers.ContentType.MediaType.Contains("json"))
                {
                    JObject jobj = JObject.Parse(payload);
                    Console.WriteLine(jobj);
                }
            }
#else
            if (response.Content != null)
            {
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            }
#endif
        }

        private static string FormatXml(string source)
        {
            StringBuilder sb = new StringBuilder();
            XmlTextWriter writer = null;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(source);

                writer = new XmlTextWriter(new StringWriter(sb));
                writer.Formatting = System.Xml.Formatting.Indented;

                doc.WriteTo(writer);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }

            return sb.ToString();
        }
    }
}