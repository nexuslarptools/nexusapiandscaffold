using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Newtonsoft.Json;
using NEXUSDataLayerScaffold.Attributes;
using NEXUSDataLayerScaffold.Entities;
using NEXUSDataLayerScaffold.Extensions;
using NEXUSDataLayerScaffold.Logic;
using NEXUSDataLayerScaffold.Models;
using Item = Minio.DataModel.Item;

namespace NEXUSDataLayerScaffold.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ImagesController : ControllerBase
{
    private readonly NexusLarpLocalContext _context;
    private readonly IMinioClient _minio;

    public ImagesController(NexusLarpLocalContext context, IMinioClient minio)
    {
        _context = context;
        _minio = minio;
    }

    // GET: api/v1/Tags
    [HttpGet]
    public async Task<ActionResult<string>> GetAllImages([OpenApiParameterIgnore] [FromHeader(Name = "Authorization")] string origin)
    {

        var accessToken = origin.Remove(0, 7);

        if (accessToken != "IAmABanana!")
        {
            return Unauthorized();
        }

        string bucket = "nexusdata";

        try
        {
            BucketExistsArgs bargs = new BucketExistsArgs()
                .WithBucket(bucket);

            // List of objects with version IDs.
            // Check whether 'mybucket' exists or not.
            var found = await _minio.BucketExistsAsync(bargs);
            if (found)
            {
                // List objects from 'my-bucketname'
                ListObjectsArgs args = new ListObjectsArgs()
                                          .WithBucket(bucket)
                                          .WithPrefix("images")
                                          .WithRecursive(true)
                                          .WithVersions(true);

                //IObservable<Item> observable = _minio.ListObjectsAsync(args);
                //IObserver<Item> observer = Observer.Create<Item>(
                //     item => Console.WriteLine("OnNext: {0} - {1}", item.Key, item.VersionId),
                //     ex => Console.WriteLine("OnError: {0}", ex.Message),
                 //     () => Console.WriteLine("OnCompleted"));
               //IDisposable subscription = observable.Subscribe(observer);

            }
            else
            {
                Console.WriteLine("mybucket does not exist");
            }


            try
            {
                // Get the metadata of the object.
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                                                    .WithBucket(bucket)
                                                    .WithObject("images/ItemSheets/Approved/d2d18353-6a4e-465d-a003-67db21eaae4d.jpg");
                ObjectStat objectStat = await _minio.StatObjectAsync(statObjectArgs);
                Console.WriteLine(objectStat);
            }
            catch (MinioException e)
            {
                Console.WriteLine("Error occurred: " + e);
            }
        }
        catch (MinioException e)
        {
            Console.WriteLine("Error occurred: " + e);
        }

        return Ok("Good?");

    }

    [HttpPut("UpdateApprovedImageLinks")]
    public async Task<ActionResult<string>> UpdateImages([OpenApiParameterIgnore][FromHeader(Name = "Authorization")] string origin)
    {
        var accessToken = origin.Remove(0, 7);

        if (accessToken != "IAmABanana!")
        {
            return Unauthorized();
        }

        string bucket = "nexusdata";

        try
        {
            var allApprovedCharacters = await _context.CharacterSheetApproveds.Where(csa => csa.Isactive == true).ToListAsync();

            foreach (var csa in allApprovedCharacters)
            {
                // Get the metadata of the object.
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                                                    .WithBucket(bucket)
                                                    .WithObject("images/Characters/"+csa.Guid.ToString()+".jpg");
                ObjectStat objectStat = await _minio.StatObjectAsync(statObjectArgs);

                csa.Img1 = objectStat.VersionId;

                // Get the metadata of the object.
                statObjectArgs = new StatObjectArgs()
                                                    .WithBucket(bucket)
                                                    .WithObject("images/Characters/" + csa.Guid.ToString() + "_2.jpg");
                objectStat = await _minio.StatObjectAsync(statObjectArgs);

                csa.Img2 = objectStat.VersionId;
            }

            var allApprovedItems = await _context.ItemSheetApproveds.Where(csa => csa.Isactive == true).ToListAsync();

            foreach (var csa in allApprovedItems)
            {
                // Get the metadata of the object.
                StatObjectArgs statObjectArgs = new StatObjectArgs()
                                                    .WithBucket(bucket)
                                                    .WithObject("images/Items/" + csa.Guid.ToString() + ".jpg");
                ObjectStat objectStat = await _minio.StatObjectAsync(statObjectArgs);

                csa.Img1 = objectStat.VersionId;
            }

            await _context.SaveChangesAsync();
        }
        catch (MinioException e)
        {
            Console.WriteLine("Error occurred: " + e);
        }

        return Ok("Updooted");

    }
}

