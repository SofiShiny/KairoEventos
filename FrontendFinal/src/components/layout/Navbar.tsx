import { useAuth } from 'react-oidc-context';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { Ticket, User, LogOut, LayoutDashboard, Search, Menu, X } from 'lucide-react';
import { useState } from 'react';
import { useT } from '../../i18n';
import { LanguageSelector } from '../LanguageSelector';

export const Navbar = () => {
    const auth = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const t = useT();

    const user = auth.user?.profile;
    const isActive = (path: string) => location.pathname === path;

    const NavLink = ({ to, children, icon: Icon }: { to: string; children: React.ReactNode; icon: any }) => (
        <Link
            to={to}
            className={`flex items-center gap-2 px-4 py-2 rounded-xl transition-all font-semibold ${isActive(to)
                ? 'bg-blue-600 text-white shadow-lg shadow-blue-500/30'
                : 'text-neutral-400 hover:text-white hover:bg-neutral-800'
                }`}
        >
            <Icon className="w-4 h-4" />
            <span>{children}</span>
        </Link>
    );

    return (
        <nav className="bg-black/80 backdrop-blur-xl border-b border-neutral-800 sticky top-0 z-50">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex justify-between h-20">
                    {/* Logo */}
                    <Link to="/" className="flex items-center gap-3 group">
                        <div className="w-10 h-10 bg-gradient-to-br from-blue-600 to-indigo-700 rounded-xl flex items-center justify-center shadow-lg group-hover:scale-110 transition-transform">
                            <span className="text-white font-black text-xl italic">K</span>
                        </div>
                        <span className="font-black text-2xl tracking-tighter text-white">KAIRO</span>
                    </Link>

                    {/* Desktop Navigation */}
                    <div className="hidden md:flex items-center gap-2">
                        <NavLink to="/" icon={Search}>{t.nav.explore}</NavLink>
                        {auth.isAuthenticated && (
                            <>
                                <NavLink to="/perfil" icon={LayoutDashboard}>{t.nav.dashboard}</NavLink>
                                <NavLink to="/entradas" icon={Ticket}>{t.nav.myTickets}</NavLink>
                            </>
                        )}
                    </div>

                    {/* Desktop Auth */}
                    <div className="hidden md:flex items-center gap-4">
                        <LanguageSelector />
                        {auth.isAuthenticated ? (
                            <div className="flex items-center gap-4 pl-4 border-l border-neutral-800">
                                <div className="text-right">
                                    <p className="text-xs font-bold text-neutral-500 uppercase tracking-widest">{t.nav.connected}</p>
                                    <p className="text-sm font-black text-white">
                                        {(user as any)?.preferred_username}
                                    </p>
                                </div>
                                <div className="relative group">
                                    <button
                                        onClick={() => navigate('/perfil')}
                                        className="w-10 h-10 rounded-full bg-neutral-800 border border-neutral-700 flex items-center justify-center hover:border-blue-500 transition-colors"
                                    >
                                        <User className="w-5 h-5 text-blue-400" />
                                    </button>
                                </div>
                                <button
                                    onClick={() => auth.signoutRedirect()}
                                    className="p-2.5 text-neutral-500 hover:text-red-500 hover:bg-red-500/10 rounded-xl transition-all"
                                    title={t.nav.logout}
                                >
                                    <LogOut className="w-5 h-5" />
                                </button>
                            </div>
                        ) : (
                            <button
                                onClick={() => auth.signinRedirect()}
                                className="px-6 py-2.5 bg-white text-black font-black rounded-xl hover:bg-blue-600 hover:text-white transition-all shadow-xl active:scale-95"
                            >
                                {t.nav.login.toUpperCase()}
                            </button>
                        )}
                    </div>

                    {/* Mobile Menu Button */}
                    <div className="md:hidden flex items-center gap-4">
                        <LanguageSelector />
                        <button
                            onClick={() => setIsMenuOpen(!isMenuOpen)}
                            className="p-2 text-neutral-400 hover:text-white bg-neutral-900 rounded-xl"
                        >
                            {isMenuOpen ? <X /> : <Menu />}
                        </button>
                    </div>
                </div>
            </div>

            {/* Mobile Menu */}
            {isMenuOpen && (
                <div className="md:hidden bg-neutral-900 border-b border-neutral-800 p-4 space-y-2 animate-in slide-in-from-top duration-300">
                    <Link to="/" onClick={() => setIsMenuOpen(false)} className="flex items-center gap-3 p-4 text-white font-bold rounded-2xl hover:bg-neutral-800">
                        <Search className="w-5 h-5" /> {t.nav.explore}
                    </Link>
                    {auth.isAuthenticated ? (
                        <>
                            <Link to="/perfil" onClick={() => setIsMenuOpen(false)} className="flex items-center gap-3 p-4 text-white font-bold rounded-2xl hover:bg-neutral-800">
                                <LayoutDashboard className="w-5 h-5" /> {t.nav.dashboard}
                            </Link>
                            <Link to="/entradas" onClick={() => setIsMenuOpen(false)} className="flex items-center gap-3 p-4 text-white font-bold rounded-2xl hover:bg-neutral-800">
                                <Ticket className="w-5 h-5" /> {t.nav.myTickets}
                            </Link>
                            <div className="pt-4 border-t border-neutral-800">
                                <button
                                    onClick={() => auth.signoutRedirect()}
                                    className="flex items-center gap-3 w-full p-4 text-red-500 font-bold rounded-2xl hover:bg-red-500/10"
                                >
                                    <LogOut className="w-5 h-5" /> {t.nav.logout}
                                </button>
                            </div>
                        </>
                    ) : (
                        <button
                            onClick={() => auth.signinRedirect()}
                            className="w-full mt-4 py-4 bg-white text-black font-black rounded-2xl"
                        >
                            {t.nav.login.toUpperCase()}
                        </button>
                    )}
                </div>
            )}
        </nav>
    );
};
