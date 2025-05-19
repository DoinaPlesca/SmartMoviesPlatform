using System.Security.Claims;
using AutoMapper;
using AuthService.Application.Dtos;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Services;

namespace AuthService.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        JwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null || !user.VerifyPassword(request.Password, _passwordHasher))
            return null;

        var token = _jwtTokenService.CreateToken(new Dictionary<string, string>
        {
            { ClaimTypes.Name, user.Username },
            { "sub", user.Id.ToString() }
        });

        var response = _mapper.Map<LoginResponseDto>(user);
        response.Token = token.Value;

        return response;
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
            return false;
        
        var tempUser = _mapper.Map<User>(request);
        var passwordHash = _passwordHasher.Hash(request.Password);
        var newUser = new User(tempUser.Username, passwordHash, tempUser.Email);

        await _userRepository.AddAsync(newUser);
        return true;
    }
}