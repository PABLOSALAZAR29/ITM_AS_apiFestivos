using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using apiFestivos.Aplicacion.Servicios;
using apiFestivos.Core.Interfaces.Repositorios;
using apiFestivos.Dominio.Entidades;


namespace apiFestivos.Test
{
    public class FestivoServicioTest
    {
        private readonly Mock<IFestivoRepositorio> _repositorioMock;
        private readonly FestivoServicio _festivoServicio;

        public FestivoServicioTest()
        {
            _repositorioMock = new Mock<IFestivoRepositorio>();
            _festivoServicio = new FestivoServicio(_repositorioMock.Object);
        }

        #region Pruebas de EsFestivo()

        [Fact]
        public async Task EsFestivo_FechaEsFestiva_RetornaTrue()
        {
            var fecha = new DateTime(2025, 1, 1); // Año Nuevo
            var festivos = new List<Festivo>
            {
                new Festivo { Id = 1, Nombre = "Año nuevo", Dia = 1, Mes = 1, IdTipo = 1, DiasPascua = 0 }
            };
            _repositorioMock.Setup(r => r.ObtenerTodos()).ReturnsAsync(festivos);

            var result = await _festivoServicio.EsFestivo(fecha);

            Assert.True(result);
        }

        [Fact]
        public async Task EsFestivo_FechaNoEsFestiva_RetornaFalse()
        {
            var fecha = new DateTime(2025, 1, 2); // No es festivo
            var festivos = new List<Festivo>
            {
                new Festivo { Id = 1, Nombre = "Año nuevo", Dia = 1, Mes = 1, IdTipo = 1, DiasPascua = 0 }
            };
            _repositorioMock.Setup(r => r.ObtenerTodos()).ReturnsAsync(festivos);

            var result = await _festivoServicio.EsFestivo(fecha);

            Assert.False(result);
        }

        #endregion

        #region Pruebas de ObtenerFestivo()

        [Fact]
        public void ObtenerFestivo_FestivoFijo_RetornaFechaCorrecta()
        {
            var festivo = new Festivo { Id = 1, Nombre = "Año nuevo", Dia = 1, Mes = 1, IdTipo = 1 };
            var año = 2025;

            var result = _festivoServicio.ObtenerFestivo(año, festivo);

            Assert.Equal(new DateTime(2025, 1, 1), result.Fecha);
        }

        [Fact]
        public void ObtenerFestivo_FestivoMovible_RetornaLunesSiguiente()
        {
            // Arrange
            var festivo = new Festivo
            {
                Id = 2,
                Nombre = "Santos Reyes",
                Dia = 6,
                Mes = 1,
                IdTipo = 2 // Tipo trasladable
            };
            var año = 2025; // 6 de enero de 2025

            // Act
            var result = _festivoServicio.ObtenerFestivo(año, festivo);

            // Assert
            Assert.Equal(result.Fecha, new DateTime(2025, 1, 13)); // Siguiente lunes
        }


        [Fact]
        public void ObtenerFestivo_FestivoBasadoEnPascua_RetornaFechaCorrecta()
        {
            var festivo = new Festivo { Id = 3, Nombre = "Domingo de Pascua", Dia = 0, Mes = 0, IdTipo = 3, DiasPascua = 3 };
            var año = 2025;

            var result = _festivoServicio.ObtenerFestivo(año, festivo);

            var domingoPascua = new DateTime(2025, 4, 20); // Domingo de Pascua 2025
            Assert.Equal(domingoPascua.AddDays(3), result.Fecha); // El 3er día después de Pascua es el 23 de abril
        }

        [Fact]
        public void ObtenerFestivo_FestivoPuenteSemanaSanta_RetornaLunesSiguiente()
        {
            var festivo = new Festivo { Id = 4, Nombre = "Ascensión del Señor", Dia = 0, Mes = 0, IdTipo = 4, DiasPascua = 40 };
            var año = 2025;

            var result = _festivoServicio.ObtenerFestivo(año, festivo);

            var domingoPascua = new DateTime(2025, 4, 20); // Domingo de Pascua 2025
            var fechaEsperada = domingoPascua.AddDays(40); // 40 días después de Pascua
            var lunesSiguiente = result.Fecha; // El lunes después del festivo

            Assert.Equal(lunesSiguiente, new DateTime(2025, 6, 2)); // Ascensión del Señor es el lunes después de los 40 días
        }

        #endregion
    }
}
