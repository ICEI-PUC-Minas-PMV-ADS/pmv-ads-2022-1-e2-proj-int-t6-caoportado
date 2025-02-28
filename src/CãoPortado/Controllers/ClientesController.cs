﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using NETCore.MailKit.Core;
using PetHotel.Data;
using PetHotel.Models;

namespace PetHotel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClientesController : Controller
    {
        private readonly Contexto _context;

        public ClientesController(Contexto context)
        {
            _context = context;
        }

        

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email,Senha,")] Clientes clientes)
        {
            var user = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Email == clientes.Email);

            if (user == null)
            {
                ViewBag.Message= "Usuario e/ou Senha Inválidos!";
                return View();
            }


            bool isSenhaOk = BCrypt.Net.BCrypt.Verify(clientes.Senha, user.Senha);

            if (isSenhaOk)
            {

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Nome),
                    new Claim("CPF", user.CPF),
                    new Claim("Id", user.Id.ToString()),
                    new Claim("Email", user.Email),
                    new Claim(ClaimTypes.Role, user.Perfil.ToString())
                };

                

                var userIdentity = new ClaimsIdentity(claims, "login");

                ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

                var props = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateAndTime.Now.ToLocalTime().AddDays(7),
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(principal, props);

                return Redirect("/");

            }

            ViewBag.Message = "Usuario e/ou Senha Inválidos!";
            return View();
            
        }


        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {

            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Clientes");


        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [AllowAnonymous]
        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var user = User.Claims.First(y => y.Type == "CPF");
            return _context.Clientes != null ?
                          View(await _context.Clientes.Where(x => x.CPF == user.Value).ToListAsync()) :
                          Problem("Entity set 'Contexto.Cliente'  is null.");
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientes = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clientes == null)
            {
                return NotFound();
            }

            return View(clientes);
        }
        [AllowAnonymous]
        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,CPF,DataDeNascimento,Endereco,Telefone,Email,Senha,Senha2")] Clientes clientes)
        {


            if (clientes.Senha != clientes.Senha2)
            {
                ViewBag.Message = "Senhas não conferem. Digite novamente";
                return View();
            }



            if (ModelState.IsValid)
            {
                clientes.Senha = BCrypt.Net.BCrypt.HashPassword(clientes.Senha);
                clientes.Senha2 = BCrypt.Net.BCrypt.HashPassword(clientes.Senha2);
                _context.Add(clientes);
                await _context.SaveChangesAsync();
                ViewBag.Message = "Cadastro realizado com sucesso!";
                //return RedirectToAction(nameof(Index));

            }


            return View();
             
        }
        [AllowAnonymous]
        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientes = await _context.Clientes.FindAsync(id);
            if (clientes == null)
            {
                return NotFound();
            }
            return View(clientes);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [AllowAnonymous]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            // Verifica se o Id da página está null (asp-route-id)
            if (id == null)
            {
                return NotFound();
            }

            // Busca no banco de dados o Pet que sofrerá a modificação
            var cadToUpdate = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
            
            if (await TryUpdateModelAsync<Clientes>(cadToUpdate, "", c => c.Endereco, c => c.Senha, c => c.Telefone))
            {
                cadToUpdate.Senha = BCrypt.Net.BCrypt.HashPassword(cadToUpdate.Senha);
                // Colocar dentro do try catch
                try
                {
                    // Aqui está executando o envio para o banco de dados
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Se der erro, irá cair aqui
                    throw;

                }
                return RedirectToAction(nameof(Index));
            }

            return View(cadToUpdate);
        }
        [AllowAnonymous]
        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var hospede = await _context.Clientes
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (hospede == null)
                {
                    return NotFound();
                }

                return View(hospede);
            }

        // POST: Hospedes/Delete/5
        [AllowAnonymous]
        [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var hospede = await _context.Clientes.FindAsync(id);
                _context.Clientes.Remove(hospede);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            private bool ClientesExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }

           
    }
}
