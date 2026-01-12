import { Outlet } from 'react-router-dom';
import { Navbar } from './Navbar';

export const MainLayout = () => {
    return (
        <div className="min-h-screen bg-black flex flex-col">
            <Navbar />
            <main className="flex-1">
                <Outlet />
            </main>
            <footer className="bg-neutral-900/50 border-t border-neutral-800 py-12 px-4">
                <div className="max-w-7xl mx-auto flex flex-col md:flex-row justify-between items-center gap-8">
                    <div className="flex items-center gap-3">
                        <div className="w-8 h-8 bg-neutral-800 rounded-lg flex items-center justify-center">
                            <span className="text-white font-black text-sm italic">K</span>
                        </div>
                        <span className="font-extrabold text-lg tracking-tighter text-white">KAIRO</span>
                    </div>
                    <p className="text-neutral-500 text-sm">
                        &copy; 2024 Kairo Events Ecosystem. Todos los derechos reservados.
                    </p>
                    <div className="flex gap-6 text-neutral-400 text-sm font-medium">
                        <a href="#" className="hover:text-white transition-colors">Privacidad</a>
                        <a href="#" className="hover:text-white transition-colors">Términos</a>
                        <a href="#" className="hover:text-white transition-colors">Soporte</a>
                    </div>
                </div>
            </footer>
        </div>
    );
};
