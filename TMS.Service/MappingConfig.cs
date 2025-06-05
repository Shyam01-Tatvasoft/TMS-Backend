using AutoMapper;
using TMS.Repository.Data;
using TMS.Repository.Dtos;

namespace TMS.Service;

public class MappingConfig:Profile
{
    public MappingConfig(){

        CreateMap<User,UserDto>().ReverseMap();
        CreateMap<User,UserRegisterDto>().ReverseMap();
    }
}
