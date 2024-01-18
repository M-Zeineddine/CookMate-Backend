


using CookMateBackend.Models.OutputModels;
using CookMateBackend.Models.ResponseResults;
using CookMateBackend.Models;
using CookMateBackend.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using CookMateBackend.Models.InputModels;

namespace CookMateBackend.Data.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        public readonly CookMateContext _CookMateContext;

        public FollowRepository(CookMateContext cookMateContext)
        {
            _CookMateContext = cookMateContext;
        }


    }
}
